using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using AutoExpense.Android.Adapters;
using AutoExpense.Android.Interfaces;
using AutoExpense.Android.Models;
using AutoExpense.Android.Services;
using Google.Android.Material.BottomSheet;
using Xamarin.Essentials;
using Uri=Android.Net.Uri;
using static AutoExpense.Android.Helpers.Constants;


namespace AutoExpense.Android.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISendersManager
    {
        private RecyclerView transactionsRecyclerView;
        private TextView timeOfDayTextView, nameTextView, totalMessagesTextView, syncedMessagesTextView;
        private Button sendersButton, syncButton;
        private TransactionsAdapter transactionsAdapter;
        private BottomSheetDialog bottomSheetDialog;
        private ImageView settingsImageView;
        public List<SMS> messages { get; set; }
        public List<string> senders { get; set; }
        public List<string> SelectSenders { get; set; }=new List<string>();
        public List<Transaction> DisplayedTransactions { get; set; } = new List<Transaction>();
        public LocalDatabaseService dbService { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjA1MjQxQDMyMzAyZTMxMmUzMExxQjBrWW1zcW83ZUQ0UFJ6VTNnOTRQdnRrTUpZOXlFa2VFUGVVdWxhSGs9");

            var path = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "AutoExpense.db3");

            dbService = new LocalDatabaseService(path);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            transactionsRecyclerView = FindViewById<RecyclerView>(Resource.Id.transactions_recyclerView);
            timeOfDayTextView = FindViewById<TextView>(Resource.Id.time_of_day_textView);
            nameTextView = FindViewById<TextView>(Resource.Id.name_textView);
            totalMessagesTextView = FindViewById<TextView>(Resource.Id.total_messages_textView);
            syncedMessagesTextView = FindViewById<TextView>(Resource.Id.synced_messages_textView);
            sendersButton = FindViewById<Button>(Resource.Id.senders_button);
            syncButton = FindViewById<Button>(Resource.Id.sync_button);
            settingsImageView = FindViewById<ImageView>(Resource.Id.settings_imageView);

            timeOfDayTextView.Text=GetTimeofDay();

            RequestPermissions(new string[] { Manifest.Permission.ReadSms }, 0);

            LoadMessages();

            transactionsAdapter = new TransactionsAdapter(DisplayedTransactions);
            transactionsRecyclerView.SetAdapter(transactionsAdapter);


            UpdateStatusCard();
            nameTextView.Text = "Josh N.";

            sendersButton.Click += SendersButton_Click;
            settingsImageView.Click += SettingsImageView_Click;
            syncButton.Click += SyncButton_Click;
        }

        private void SyncButton_Click(object sender, EventArgs e)
        {
            var luisAppId = Preferences.Get(LUIS_APP_ID, null);
            var luisSubscriptionKey = Preferences.Get(LUIS_SUBBSCRIPTION_KEY, null);
            var ynabAccessToken = Preferences.Get(YNAB_ACCESS_TOKEN, null);
            var endpointUrl = Preferences.Get(ENDPOINT_URL, null);

            if (string.IsNullOrEmpty(luisAppId) || string.IsNullOrEmpty(luisSubscriptionKey) ||
                string.IsNullOrEmpty(ynabAccessToken) || string.IsNullOrEmpty(endpointUrl))
            {
                StartActivity(typeof(SettingsActivity));
                return;
            }

            SyncTransactions(luisAppId, luisSubscriptionKey, ynabAccessToken, endpointUrl);
        }

        private async void SyncTransactions(string luisAppId, string luisSubscriptionKey, string ynabAccessToken, string endpointUrl)
        {
            var luisPredictionService = new LuisPredictionService(luisAppId, luisSubscriptionKey, endpointUrl);
            var temps = DisplayedTransactions.Select(d => d).ToList();
            foreach (var transaction in temps)
            {
                if (transaction.Amount != null)
                    continue;

                var prediction= await luisPredictionService.GetPrediction(transaction.Body);
                if (prediction is null)
                {
                    continue;
                }

                if (prediction.Prediction.TopIntent == "CashOutflow" || prediction.Prediction.TopIntent == "CashInflow" || prediction.Prediction.TopIntent== "Fuliza")
                {

                    var transactionType =
                        (TransactionType) Enum.Parse(typeof(TransactionType), prediction.Prediction.TopIntent);

                    var index = temps.IndexOf(transaction);

                    var stringAmt = prediction.Prediction.Entities.Amount?.FirstOrDefault()?.Trim().Remove(0,3);
                    if(string.IsNullOrEmpty(stringAmt))
                        continue;

                    if (prediction.Prediction.Entities.TransactionCost != null)
                    {
                        var stringTrCost = prediction.Prediction.Entities.TransactionCost.FirstOrDefault()?.Trim().Remove(0, 3);
                        DisplayedTransactions[index].TransactionCost = float.Parse(stringTrCost ?? string.Empty);
                    }

                    var principal = prediction.Prediction.Entities.Principal;

                    if (principal != null)
                    {
                        DisplayedTransactions[index].MessageSender = principal.FirstOrDefault() ?? string.Empty;
                        DisplayedTransactions[index].Principal = prediction.Prediction.Entities.Principal.FirstOrDefault() ?? string.Empty;
                    }

                    DisplayedTransactions[index].Amount = float.Parse(stringAmt ?? string.Empty);
                    DisplayedTransactions[index].Code = prediction.Prediction.Entities.Code?.FirstOrDefault() ?? string.Empty;
                    DisplayedTransactions[index].TransactionType = transactionType;

                    transactionsAdapter.NotifyItemChanged(index);

                    dbService.SaveTransactionPrediction(new TPrediction(transaction.ThreadId, transaction.Id,
                        transaction.Date,
                        transaction.MessageSender, transaction.TransactionType, transaction.Amount,
                        transaction.TransactionCost, transaction.Code, transaction.Principal));


                }

                else
                {
                    continue;
                }
            }
        }

        private void SettingsImageView_Click(object sender, EventArgs e)
        {
           StartActivity(typeof(SettingsActivity));
        }

        private void LoadMessages()
        {
            messages = GetAllSms();

            var sendersString = Preferences.Get(SENDERS_LIST, null);
            SelectSenders = string.IsNullOrEmpty(sendersString)
                ? new List<string>()
                : sendersString.Split(",").Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();


            foreach (var message in messages)
            {
                if (SelectSenders.Contains(message.Address))
                {
                    var tPredictions = dbService.GetTransactionPredictions();
                    var tPrediction =
                        tPredictions.FirstOrDefault(t => t.Id == message.Id && t.ThreadId == message.ThreadId);

                    if (tPrediction is null)
                    {
                        var transaction = new Transaction(message.Address, message.Date, null, null, null, null, null,
                            message.Body, message.ThreadId, message.Id);
                        
                        DisplayedTransactions.Add(transaction);
                        continue;
                    }
                    else
                    {
                        var transaction = new Transaction(tPrediction.MessageSender, message.Date, tPrediction.TransactionType, tPrediction.Amount, tPrediction.TransactionCost, tPrediction.Code, tPrediction.Principal,
                            message.Body, message.ThreadId, message.Id);

                        DisplayedTransactions.Add(transaction);
                        continue;
                    }


                }
            }

        }

        private void UpdateStatusCard()
        {
            totalMessagesTextView.Text = DisplayedTransactions.Count.ToString();
            syncedMessagesTextView.Text = "0";
        }


        private void SendersButton_Click(object sender, EventArgs e)
        {
            senders = messages.Where(s => !s.Address.StartsWith("+")).Select(s => s.Address).Distinct().ToList();

            
            bottomSheetDialog = new BottomSheetDialog(this);
            bottomSheetDialog.SetContentView(Resource.Layout.dialog_senders);

            var recyclerView = bottomSheetDialog.FindViewById<RecyclerView>(Resource.Id.senders_recyclerView);
            var cancelButton = bottomSheetDialog.FindViewById<Button>(Resource.Id.cancel_button);
            var updateButton = bottomSheetDialog.FindViewById<Button>(Resource.Id.update_button);

            var adapter = new SendersAdapter(senders, SelectSenders, this);
            recyclerView?.SetAdapter(adapter);

            cancelButton.Click += (s, e) => bottomSheetDialog.Dismiss();
            updateButton.Click += UpdateButton_Click;

            bottomSheetDialog.Show();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            var sendersString = string.Empty;
            foreach (var selectSender in SelectSenders)
            {
                if(sendersString.Contains(selectSender))
                    continue;

                sendersString = $"{sendersString},{selectSender}";
            }
            Preferences.Set(SENDERS_LIST,sendersString );

            DisplayedTransactions.Clear();

            foreach (var message in messages)
            {
                if (SelectSenders.Contains(message.Address))
                {
                    var transaction = new Transaction(message.Address, message.Date, null, null, null, null, null,
                        message.Body, message.ThreadId, message.Id);

                    DisplayedTransactions.Add(transaction);
                }
                else
                {
                    continue;
                }

            }

            transactionsAdapter.NotifyDataSetChanged();
            bottomSheetDialog.Dismiss();

            UpdateStatusCard();
        }

        private string GetTimeofDay()
        {
            var greeting = "";
            var hour = DateTime.Now.Hour;
            if (hour < 12)
            {
                greeting = "Good Morning";
            }
            else if (hour < 18)
            {
                greeting = "Good Afternoon";
            }
            else
            {
                greeting = "Good Evening";
            }
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
            if (id == Resource.Id.action_sync_all)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private List<SMS> GetAllSms()
        {
            var inbox = "content://sms/inbox";
            var reqCols = new[] { "_id", "thread_id", "address", "person", "date", "body", "type" };
            var uri = Uri.Parse(inbox);
            var cursor = ContentResolver?.Query(uri, reqCols, null, null, null);

            var messages = new List<SMS>();

            if (cursor.MoveToFirst())
            {
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

            }

            return messages;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void SenderSelected(string sender)
        {
            SelectSenders.Add(sender);
        }

        public void SenderRemoved(string sender)
        {
            SelectSenders.Remove(sender);
        }
    }
}