using Microsoft.Datasync.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoExpense.Android.Events;
using AutoExpense.Android.Helpers;
using AutoExpense.Android.Interfaces;
using AutoExpense.Android.Models;
using Microsoft.Datasync.Client.SQLiteStore;

namespace AutoExpense.Android.Services
{
    /// <summary>
    /// An implementation of the <see cref="ITransactionService"/> interface that uses
    /// a remote table on a Datasync Service.
    /// </summary>
    public class RemoteTransactionService : ITransactionService
    {
        /// <summary>
        /// Reference to the client used for datasync operations.
        /// </summary>
        private DatasyncClient _client = null;

        /// <summary>
        /// Reference to the table used for datasync operations.
        /// </summary>
        private IOfflineTable<SyncTransaction> _table = null;

        /// <summary>
        /// When set to true, the client and table and both initialized.
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Used for locking the initialization block to ensure only one initialization happens.
        /// </summary>
        private readonly SemaphoreSlim _asyncLock = new(1, 1);

        /// <summary>
        /// An event handler that is triggered when the list of items changes.
        /// </summary>
        public event EventHandler<TransactionServiceEventArgs> TransactionsUpdated;

        /// <summary>
        /// When using authentication, the token requestor to use.
        /// </summary>
        public Func<Task<AuthenticationToken>> TokenRequestor;

        /// <summary>
        /// Creates a new <see cref="RemoteTransactionService"/> with no authentication.
        /// </summary>
        public RemoteTransactionService()
        {
            TokenRequestor = null; // no authentication
        }

        /// <summary>
        /// Creates a new <see cref="RemoteTodoService"/> with authentication.
        /// </summary>
        public RemoteTransactionService(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        /// <summary>
        /// The path to the offline database
        /// </summary>
        public string OfflineDb { get; set; }

        /// <summary>
        /// Initialize the connection to the remote table.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeAsync()
        {
            // Create the offline store definition
            var connectionString = new UriBuilder { Scheme = "file", Path = OfflineDb, Query = "?mode=rwc" }.Uri.ToString();
            var store = new OfflineSQLiteStore(connectionString);
            store.DefineTable<SyncTransaction>();
            var options = new DatasyncClientOptions
            {
                OfflineStore = store,
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Create the datasync client.
            _client = TokenRequestor == null
                ? new DatasyncClient(Constants.BackendUrl, options)
                : new DatasyncClient(Constants.BackendUrl, new GenericAuthenticationProvider(TokenRequestor), options);

            // Initialize the database
            await _client.InitializeOfflineStoreAsync();

            // Get a reference to the offline table.
            _table = _client.GetOfflineTable<SyncTransaction>();

            // Set _initialized to true to prevent duplication of locking.
            _initialized = true;
        }

        /// <summary>
        /// Get all the items in the list.
        /// </summary>
        /// <returns>The list of items (asynchronously)</returns>
        public async Task<List<SyncTransaction>> GetItemsAsync()
        {
            await InitializeAsync();
            return await _table.GetAsyncItems().ToListAsync();
        }

        /// <summary>
        /// Refreshes the TodoItems list manually.
        /// </summary>
        /// <returns>A task that completes when the refresh is done.</returns>
        public async Task RefreshItemsAsync()
        {
            await InitializeAsync();

            // First, push all the items in the table.
            await _table.PushItemsAsync();

            // Then, pull all the items in the table.
            await _table.PullItemsAsync();

            return;
        }

        /// <summary>
        /// Removes an item in the list, if it exists.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>A task that completes when the item is removed.</returns>
        public async Task RemoveItemAsync(SyncTransaction item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Id == null)
            {
                // Short circuit for when the item has not been saved yet.
                return;
            }
            await InitializeAsync();
            await _table.DeleteItemAsync(item);
            TransactionsUpdated?.Invoke(this, new TransactionServiceEventArgs(TransactionServiceEventArgs.ListAction.Delete, item));
        }

        /// <summary>
        /// Saves an item to the list.  If the item does not have an Id, then the item
        /// is considered new and will be added to the end of the list.  Otherwise, the
        /// item is considered existing and is replaced.
        /// </summary>
        /// <param name="item">The new item</param>
        /// <returns>A task that completes when the item is saved.</returns>
        public async Task SaveItemAsync(SyncTransaction item)
        {
            if (item == null)
            {
                throw new ArgumentException(nameof(item));
            }

            await InitializeAsync();

            TransactionServiceEventArgs.ListAction action = (item.Id == null) ? TransactionServiceEventArgs.ListAction.Add : TransactionServiceEventArgs.ListAction.Update;
            if (item.Id == null)
            {
                await _table.InsertItemAsync(item);
            }
            else
            {
                await _table.ReplaceItemAsync(item);
            }
            TransactionsUpdated?.Invoke(this, new TransactionServiceEventArgs(action, item));
        }
    }
}