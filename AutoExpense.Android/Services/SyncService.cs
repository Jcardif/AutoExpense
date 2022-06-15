using Microsoft.WindowsAzure.MobileServices;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoExpense.Android.Events;
using AutoExpense.Android.Helpers;
using AutoExpense.Android.Interfaces;
using AutoExpense.Android.Models;
using AutoExpense.Shared.Helpers;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace AutoExpense.Android.Services
{
    public class SyncService : ITransactionService
    {
        private bool isInitialized = false;
        private readonly AsyncLock initializationLock = new AsyncLock();

        private MobileServiceClient mClient;
        // private IMobileServiceTable<SyncTransaction> mTable;
        private IMobileServiceSyncTable<SyncTransaction> mTable;
        // private MobileServiceSQLiteStore mStore;

        /// <summary>
        /// An event handler that is called whenever the repository is updated
        /// </summary>
        public event EventHandler<TransactionServiceEventArgs> TransactionsUpdated;

        private async Task InitializeAsync()
        {
            using (await initializationLock.LockAsync())
            {
                if (!isInitialized)
                {
                    mClient = new MobileServiceClient(Constants.BackendUrl, new LoggingHandler());


                    var fileName = "autoexpense.db";
                    var store = new MobileServiceSQLiteStore(fileName);
                    store.DefineTable<SyncTransaction>();

                    await mClient.SyncContext.InitializeAsync(store);

                    mTable = mClient.GetSyncTable<SyncTransaction>();
                    isInitialized = true;
                }
            }
        }

        private void OnTodoListChanged(ListAction action, SyncTransaction item)
            => TransactionsUpdated?.Invoke(this, new TransactionServiceEventArgs(action, item));


        public async Task<List<SyncTransaction>> GetItemsAsync()
        {
            await InitializeAsync();
            return await mTable.ToListAsync();
        }

        public async Task RefreshItemsAsync()
        {
            await InitializeAsync();

            await mClient.SyncContext.PushAsync();
            await mTable.PullAsync("allItems", mTable.CreateQuery());
        }

        public Task RemoveItemAsync(SyncTransaction item)
        {
            throw new NotImplementedException();
        }

        public async Task SaveItemAsync(SyncTransaction item)
        {
            await InitializeAsync();
            await mTable.InsertAsync(item);
            await RefreshItemsAsync();
        }

    }
}