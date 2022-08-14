using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using AutoExpense.Android.Adapters;
using AutoExpense.Android.Extensions;
using AutoExpense.Android.Interfaces;
using AutoExpense.Android.Models;
using Google.Android.Material.BottomSheet;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Syncfusion.Licensing;
using Xamarin.Essentials;
using static AutoExpense.Android.Helpers.Constants;
using Platform = Xamarin.Essentials.Platform;
using Uri = Android.Net.Uri;

#nullable disable

namespace AutoExpense.Android.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISendersManager, IItemClickListener
    {
        private BottomSheetDialog _bottomSheetDialog;
        private ImageView _deleteTransactionImageView;

        private string _endpointUrl;
        private string _luisAppId;
        private string _luisSubscriptionKey;
        private bool _saveToYnab;
        private Button _sendersButton, _syncButton;
        private ImageView _settingsImageView;
        private ProgressBar _syncingProgressBar;
        private TextView _timeOfDayTextView, _nameTextView, _totalMessagesTextView, _syncedMessagesTextView;
        private TransactionsAdapter _transactionsAdapter;
        private RecyclerView _transactionsRecyclerView;
        private string _ynabAccessToken;
        private CardView _ynabCardView;


        private List<SMS> Messages { get; set; }
        private List<string> Senders { get; set; }
        private List<string> SelectSenders { get; set; } = new();
        private List<LocalTransaction> DisplayedTransactions { get; } = new();

        private bool InSelectionMode { get; set; }

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            // TODO Move keys to separate file
            SyncfusionLicenseProvider.RegisterLicense(
                "NjA1MjQxQDMyMzAyZTMxMmUzMExxQjBrWW1zcW83ZUQ0UFJ6VTNnOTRQdnRrTUpZOXlFa2VFUGVVdWxhSGs9");
            AppCenter.Start("7e7c2c36-1ea9-499e-8d3f-96158e8924f8", typeof(Analytics), typeof(Crashes));

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            _transactionsRecyclerView = FindViewById<RecyclerView>(Resource.Id.transactions_recyclerView);
            _timeOfDayTextView = FindViewById<TextView>(Resource.Id.time_of_day_textView);
            _nameTextView = FindViewById<TextView>(Resource.Id.name_textView);
            _totalMessagesTextView = FindViewById<TextView>(Resource.Id.total_messages_textView);
            _syncedMessagesTextView = FindViewById<TextView>(Resource.Id.synced_messages_textView);
            _sendersButton = FindViewById<Button>(Resource.Id.senders_button);
            _syncButton = FindViewById<Button>(Resource.Id.sync_button);
            _settingsImageView = FindViewById<ImageView>(Resource.Id.settings_imageView);
            _syncingProgressBar = FindViewById<ProgressBar>(Resource.Id.syncing_progress_bar);
            _ynabCardView = FindViewById<CardView>(Resource.Id.ynab_card);
            _deleteTransactionImageView = FindViewById<ImageView>(Resource.Id.delete_imageView);

            // Update time of day text view
            _timeOfDayTextView.Text = GetTimeofDay();

            // Initialise and set adapter for the transactions RecyclerView
            _transactionsAdapter = new TransactionsAdapter(DisplayedTransactions, this);
            _transactionsRecyclerView.SetAdapter(_transactionsAdapter);

            // Request all relevant permissions
            // todo handle when permission is denied
            RequestPermissions(new[] { Manifest.Permission.ReadSms }, 0);

            // Fetch and display text messages in the app
            Messages = GetAllSms();
            await DisplayMessages();

            // Fetch the various app settings i.e. luis settings, ynab configs
            GetAppSettings();

            // update stats
            UpdateStatusCard();
            _nameTextView.Text = "Josh N.";

            // subscribe to events
            _sendersButton.Click += SendersButton_Click;
            _settingsImageView.Click += SettingsImageView_Click;
            _syncButton.Click += SyncButton_Click;
            _ynabCardView.Click +=  (s, e) =>
            {
                // todo navigate to ynab app
            };
            _deleteTransactionImageView.Click += DeleteTransactionImageViewClick;
        }

        /// <summary>
        ///     Handle when item in the recyclerView is clicked. 
        /// </summary>
        /// <param name="itemView"></param>
        /// <param name="position"></param>
        /// <param name="isLongClick"></param>
        public void OnItemClick(View itemView, int position, bool isLongClick)
        {
            var transaction = DisplayedTransactions[position];
            if (transaction.IsSelected)
            {
                if (isLongClick) return;

                DisplayedTransactions[position].IsSelected = false;
                _transactionsAdapter?.NotifyItemChanged(position);
            }
            else
            {
                if (isLongClick)
                {
                    InSelectionMode = true;
                    DisplayedTransactions[position].IsSelected = true;
                    _transactionsAdapter?.NotifyItemChanged(position);
                }

                else
                {
                    if (!InSelectionMode) return;
                    DisplayedTransactions[position].IsSelected = true;
                    _transactionsAdapter?.NotifyItemChanged(position);

                    return;
                }
            }
            
            InSelectionMode = DisplayedTransactions.Any(t => t.IsSelected);

            _deleteTransactionImageView.Visibility = InSelectionMode ? ViewStates.Visible : ViewStates.Gone;
        }

        public void SenderSelected(string sender)
        {
            SelectSenders.Add(sender);
        }

        public void SenderRemoved(string sender)
        {
            SelectSenders.Remove(sender);
        }



        private void GetAppSettings()
        {
            _luisAppId = Preferences.Get(LUIS_APP_ID, null);
            _luisSubscriptionKey = Preferences.Get(LUIS_SUBBSCRIPTION_KEY, null);
            _ynabAccessToken = Preferences.Get(YNAB_ACCESS_TOKEN, null);
            _endpointUrl = Preferences.Get(ENDPOINT_URL, null);
            _saveToYnab = Preferences.Get(SAVE_TO_YNAB, false);
        }

        private async void SyncButton_Click(object sender, EventArgs e)
        {
            GetAppSettings();

            if (string.IsNullOrEmpty(_luisAppId) || string.IsNullOrEmpty(_luisSubscriptionKey) ||
                string.IsNullOrEmpty(_ynabAccessToken) || string.IsNullOrEmpty(_endpointUrl))
            {
                StartActivity(typeof(SettingsActivity));
                return;
            }

            var budgetId = Preferences.Get(YNAB_SYNC_BUDGET_ID, null);
            if (string.IsNullOrEmpty(budgetId))
            {
                StartActivity(typeof(SettingsActivity));
                return;
            }

            SyncTransactions(_luisAppId, _luisSubscriptionKey, _ynabAccessToken, _endpointUrl, _saveToYnab);
        }

        /// <summary>
        ///     Synchronise offline database with remote database and create new records from the messages on the device
        ///     for all new records. 
        /// </summary>
        /// <param name="luisAppId"></param>
        /// <param name="luisSubscriptionKey"></param>
        /// <param name="ynabAccessToken"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="saveToYnab"></param>
        private async void SyncTransactions(string luisAppId, string? luisSubscriptionKey, string? ynabAccessToken,
            string endpointUrl, bool saveToYnab)
        {
            _syncButton.Enabled = false;
            _syncingProgressBar.Visibility = ViewStates.Visible;
            _syncingProgressBar.Enabled = true;


            //todo sync transactions


            _syncButton.Enabled = true;
            _syncingProgressBar.Visibility = ViewStates.Gone;
            _syncingProgressBar.Enabled = false;
        }

        private void SettingsImageView_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(SettingsActivity));
        }

        /// <summary>
        ///     Select what messages to display filtering by sender
        /// </summary>
        /// <returns></returns>
        private List<SMS> FilterMessagesBySenders()
        {
            Messages = GetAllSms();

            var sendersString = Preferences.Get(SENDERS_LIST, null);

            if (string.IsNullOrEmpty(sendersString)) return Messages;

            SelectSenders = sendersString.Split(",").Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            return Messages.Where(m => sendersString.Contains(m.Address)).ToList();
        }


        /// <summary>
        ///     Display total messages selected on device and the number of the same that is synced. 
        /// </summary>
        private void UpdateStatusCard()
        {
            _totalMessagesTextView.Text = DisplayedTransactions.Count.ToString();
            _syncedMessagesTextView.Text = "0";
        }

        /// <summary>
        ///     Display all senders and select required senders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendersButton_Click(object sender, EventArgs e)
        {
            Messages = GetAllSms();
            Senders = Messages?.Where(s => !s.Address.StartsWith("+")).Select(s => s.Address).Distinct().ToList();


            _bottomSheetDialog = new BottomSheetDialog(this);
            _bottomSheetDialog.SetContentView(Resource.Layout.dialog_senders);

            var recyclerView = _bottomSheetDialog.FindViewById<RecyclerView>(Resource.Id.senders_recyclerView);
            var cancelButton = _bottomSheetDialog.FindViewById<Button>(Resource.Id.cancel_button);
            var updateButton = _bottomSheetDialog.FindViewById<Button>(Resource.Id.update_button);

            var adapter = new SendersAdapter(Senders, SelectSenders, this);
            recyclerView?.SetAdapter(adapter);

            cancelButton.Click += (s, e) => _bottomSheetDialog.Dismiss();
            updateButton.Click += UpdateButton_Click;

            _bottomSheetDialog.Show();
        }


        /// <summary>
        ///     Display the text messages from the device and also those saved from the database
        /// </summary>
        /// <returns></returns>
        private async Task DisplayMessages()
        {
            Messages = FilterMessagesBySenders();
            foreach (var message in Messages)
            {
                var messageBodyHash = message.Body.ToHashedStringValue();

                var transaction = new LocalTransaction(message.Address, message.Date, null, null, null, null, null,
                    message.Body, message.ThreadId, message.Id, false, false, messageBodyHash);

                DisplayedTransactions.Add(transaction);
                _transactionsAdapter?.NotifyItemInserted(DisplayedTransactions.Count - 1);
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            var sendersString = string.Empty;
            foreach (var selectSender in SelectSenders)
            {
                if (sendersString.Contains(selectSender))
                    continue;

                sendersString = $"{sendersString},{selectSender}";
            }

            Preferences.Set(SENDERS_LIST, sendersString);

            DisplayedTransactions.Clear();

            await DisplayMessages();

            _bottomSheetDialog?.Dismiss();

            UpdateStatusCard();
        }

        private string? GetTimeofDay()
        {
            var hour = DateTime.Now.Hour;
            string? greeting;
            if (hour < 12)
                greeting = "Good Morning";
            else if (hour < 18)
                greeting = "Good Afternoon";
            else
                greeting = "Good Evening";
            return greeting;
        }

        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_sync_all) return true;

            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        ///     Read all text messages on the device
        /// </summary>
        /// <returns>
        ///     List of <see cref="SMS" />
        /// </returns>
        private List<SMS> GetAllSms()
        {
            const string inbox = "content://sms/inbox";
            var reqCols = new[] { "_id", "thread_id", "address", "person", "date", "body", "type" };
            var uri = Uri.Parse(inbox);

            var cursor = ContentResolver?.Query(uri, reqCols, null, null, null);

            var messages = new List<SMS>();

            if (cursor == null || !cursor.MoveToFirst()) return messages;

            do
            {
                var messageId = cursor.GetString(cursor.GetColumnIndex(reqCols[0]));
                var threadId = cursor.GetString(cursor.GetColumnIndex(reqCols[1]));
                var address = cursor.GetString(cursor.GetColumnIndex(reqCols[2]));
                var name = cursor.GetString(cursor.GetColumnIndex(reqCols[3]));
                var date = cursor.GetLong(cursor.GetColumnIndex(reqCols[4]));
                var msg = cursor.GetString(cursor.GetColumnIndex(reqCols[5]));
                var type = cursor.GetString(cursor.GetColumnIndex(reqCols[6]));

                var sms = new SMS(messageId, address, date, msg, threadId, name, type);
                messages.Add(sms);
            } while (cursor.MoveToNext());

            return messages;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void DeleteTransactionImageViewClick(object sender, EventArgs e)
        {
            _syncingProgressBar.Visibility = ViewStates.Visible;

            //Todo: handle deletion
            _syncingProgressBar.Visibility = ViewStates.Gone;
        }
    }
}