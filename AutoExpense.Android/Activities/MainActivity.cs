using System;
using System.Collections.Generic;
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
using Uri = Android.Net.Uri;
using static AutoExpense.Android.Helpers.Constants;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using AndroidX.CardView.Widget;
using Android.Graphics;
using Path = System.IO.Path;
using Android.Icu.Text;
using AutoExpense.Shared.Helpers;
using static  AutoExpense.Android.Extensions.ExtensionMethods;

namespace AutoExpense.Android.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISendersManager, IItemClickListener
    {
        private RecyclerView? transactionsRecyclerView;
        private TextView? timeOfDayTextView, nameTextView, totalMessagesTextView, syncedMessagesTextView;
        private Button? sendersButton, syncButton;
        private TransactionsAdapter? transactionsAdapter;
        private BottomSheetDialog? bottomSheetDialog;
        private ImageView? settingsImageView;
        public List<SMS>? messages { get; set; }
        public List<string>? senders { get; set; }
        public List<string> SelectSenders { get; set; }=new List<string>();
        public List<LocalTransaction> DisplayedTransactions { get; set; } = new List<LocalTransaction>();
        public SyncService RemoteTransactionService { get; set; }

        public YnabDataService YnabDataService { get; set; }
        private ProgressBar? syncingProgressBar;
        private CardView? ynabCardView;
        private ImageView deleteTransactionImageview;

        private string ynabAccessToken;
        private string endpointUrl;
        private bool saveToYnab;
        private string luisAppId;
        private string luisSubscriptionKey;

        public bool InSelectionMode { get; set; } = false;

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjA1MjQxQDMyMzAyZTMxMmUzMExxQjBrWW1zcW83ZUQ0UFJ6VTNnOTRQdnRrTUpZOXlFa2VFUGVVdWxhSGs9");
            AppCenter.Start("7e7c2c36-1ea9-499e-8d3f-96158e8924f8", typeof(Analytics), typeof(Crashes));

            RemoteTransactionService = new SyncService();

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
            syncingProgressBar = FindViewById<ProgressBar>(Resource.Id.syncing_progress_bar);
            ynabCardView = FindViewById<CardView>(Resource.Id.ynab_card);
            deleteTransactionImageview = FindViewById<ImageView>(Resource.Id.delete_imageView);

            timeOfDayTextView.Text=GetTimeofDay();

            RequestPermissions(new string[] { Manifest.Permission.ReadSms }, 0);

            messages = GetAllSms();
            await DisplayMessages();

            transactionsAdapter = new TransactionsAdapter(DisplayedTransactions, this);
            transactionsRecyclerView?.SetAdapter(transactionsAdapter);

            GetAppSettings();

            UpdateStatusCard();
            nameTextView.Text = "Josh N.";

            sendersButton.Click += SendersButton_Click;
            settingsImageView.Click += SettingsImageView_Click;
            syncButton.Click += SyncButton_Click;
            ynabCardView.Click += async (s, e) => await ShowYnabBottomSheetDialog();
            deleteTransactionImageview.Click += DeleteTransactionImageview_Click;
        }

        private void GetAppSettings()
        {
            luisAppId = Preferences.Get(LUIS_APP_ID, null);
            luisSubscriptionKey = Preferences.Get(LUIS_SUBBSCRIPTION_KEY, null);
            ynabAccessToken = Preferences.Get(YNAB_ACCESS_TOKEN, null);
            endpointUrl = Preferences.Get(ENDPOINT_URL, null);
            saveToYnab = Preferences.Get(SAVE_TO_YNAB, false);
        }

        private async void SyncButton_Click(object sender, EventArgs e)
        {
            GetAppSettings();

            if (string.IsNullOrEmpty(luisAppId) || string.IsNullOrEmpty(luisSubscriptionKey) ||
                string.IsNullOrEmpty(ynabAccessToken) || string.IsNullOrEmpty(endpointUrl))
            {
                StartActivity(typeof(SettingsActivity));
                return;
            }

            var budgetId = Preferences.Get(YNAB_SYNC_BUDGET_ID, null);
            if (string.IsNullOrEmpty(budgetId))
            {
                await ShowYnabBottomSheetDialog();
                return;
            }

            SyncTransactions(luisAppId, luisSubscriptionKey, ynabAccessToken, endpointUrl, saveToYnab);
        }

        private async void SyncTransactions(string luisAppId, string luisSubscriptionKey, string ynabAccessToken, string endpointUrl, bool saveToYnab)
        {
            syncButton.Enabled = false;
            syncingProgressBar.Visibility = ViewStates.Visible;
            syncingProgressBar.Enabled = true;

            var luisPredictionService = new LuisPredictionService(luisAppId, luisSubscriptionKey, endpointUrl);

            var temps = DisplayedTransactions.Select(d => d).ToList();

            foreach (var localTransaction in temps)
            {
                if (localTransaction.IsInDatabase)
                    continue;

                var prediction = await luisPredictionService.GetPrediction(localTransaction.Body);
                if (prediction is null)
                    continue;

                if (prediction.Prediction.TopIntent is "CashOutflow" or "CashInflow" or "Fuliza")
                {
                    var transactionType =
                        (TransactionType)Enum.Parse(typeof(TransactionType), prediction.Prediction.TopIntent);

                    var index = temps.IndexOf(localTransaction);

                    var stringAmt = prediction.Prediction.Entities.Amount?.FirstOrDefault()?.Trim().Remove(0, 3);
                    if (string.IsNullOrEmpty(stringAmt))
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
                        DisplayedTransactions[index].Principal = (prediction.Prediction.Entities.Principal ?? new List<string>()).FirstOrDefault() ?? string.Empty;
                    }

                    DisplayedTransactions[index].Amount = float.Parse(stringAmt ?? string.Empty);
                    DisplayedTransactions[index].Code = prediction.Prediction.Entities.Code?.FirstOrDefault() ?? string.Empty;
                    DisplayedTransactions[index].TransactionType = transactionType;

                    transactionsAdapter?.NotifyItemChanged(index);

                    if (saveToYnab is false)
                    {
                        var syncTransaction = new SyncTransaction(localTransaction.ThreadId, localTransaction.Date,
                            localTransaction.MessageSender,
                            localTransaction.TransactionType, localTransaction.Amount, localTransaction.TransactionCost,
                            localTransaction.Code, localTransaction.Principal, YnabSyncStatus.Skipped,
                            localTransaction.MessageHash);

                        syncTransaction.Id = localTransaction.MessageHash.Substring(0, 8);

                        await RemoteTransactionService.SaveItemAsync(syncTransaction);
                        await RemoteTransactionService.RefreshItemsAsync();
                    }

                    else
                    {
                        var syncTransaction = new SyncTransaction(localTransaction.ThreadId, localTransaction.Date,
                            localTransaction.MessageSender,
                            localTransaction.TransactionType, localTransaction.Amount, localTransaction.TransactionCost,
                            localTransaction.Code, localTransaction.Principal,
                            YnabSyncStatus.Synced, localTransaction.MessageHash);

                        syncTransaction.Id = localTransaction.MessageHash.Substring(0, 8);

                        await RemoteTransactionService.SaveItemAsync(syncTransaction);
                        await RemoteTransactionService.RefreshItemsAsync();

                        YnabDataService = new YnabDataService(ynabAccessToken);

                        var date = new SimpleDateFormat("YYYY-MM-dd").Format(localTransaction.Date);
                        var subTransactions = new List<Subtransaction>();
                        var memo = localTransaction.Body.Length > 200 ? localTransaction.Body.Substring(0, 200) : localTransaction.Body;
                        var amount =
                            localTransaction.TransactionType == TransactionType.Fuliza ||
                            localTransaction.TransactionType == TransactionType.CashOutflow
                                ? localTransaction.Amount * -1
                                : localTransaction.Amount;
                        var amt = (int)Math.Round(amount ?? 0) * 1000;

                        var budgetId = Preferences.Get(YNAB_SYNC_BUDGET_ID, null);
                        var mpesaAccountId = budgetId == "59da31b6-115c-42c5-b5bc-97dfa8b2fe1c"
                            ? "675d0b58-4359-4ef8-a19e-35fc9b2a2458"
                            : "6622a12a-5d30-4eb4-baff-d780e059793c";

                        if (localTransaction.TransactionCost != null && localTransaction.TransactionCost != 0)
                        {
                            var tcost = (int)Math.Round(localTransaction.TransactionCost * -1000 ?? 0);
                            var subTransactionCost = new Subtransaction(tcost, null, "Safaricom Transaction Costs", null, memo);
                            subTransactions.Add(subTransactionCost);

                            var subTransactionAmt = new Subtransaction(amt, null, localTransaction.Principal, null, memo);
                            subTransactions.Add(subTransactionAmt);

                            amt += tcost;

                            var trnsn1 = new Transaction(mpesaAccountId, date, amt, null, localTransaction.Principal, null, null, "cleared", true, null, null, subTransactions);
                            var ynabTransaction1 = new YnabTransaction(trnsn1);


                            await YnabDataService.SaveTransactionAsync(ynabTransaction1, budgetId);
                            continue;
                        }

                        var trnsn = new Transaction(mpesaAccountId, date, amt, null, localTransaction.Principal, null, memo, "cleared", true, null, null, subTransactions);
                        var ynabTransaction = new YnabTransaction(trnsn);

                        await YnabDataService.SaveTransactionAsync(ynabTransaction, budgetId);
                    }
                }
            }


            syncButton.Enabled = true;
            syncingProgressBar.Visibility = ViewStates.Gone;
            syncingProgressBar.Enabled = false;
        }

        private void SettingsImageView_Click(object sender, EventArgs e)
        {
           StartActivity(typeof(SettingsActivity));
        }

        private List<SMS> FilterMessagesBySenders()
        {
            messages = GetAllSms();

            var sendersString = Preferences.Get(SENDERS_LIST, null);

            if(string.IsNullOrEmpty(sendersString))
            {
                return messages;
            }

            SelectSenders = sendersString.Split(",").Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            return messages.Where(m => sendersString.Contains(m.Address)).ToList();
        }
        
        private void UpdateStatusCard()
        {
            totalMessagesTextView.Text = DisplayedTransactions.Count.ToString();
            syncedMessagesTextView.Text = "0";
        }


        private void SendersButton_Click(object sender, EventArgs e)
        {
            messages = GetAllSms();
            senders = messages?.Where(s => !s.Address.StartsWith("+")).Select(s => s.Address).Distinct().ToList();

            
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

        private async Task ShowYnabBottomSheetDialog()
        {
            GetAppSettings();

            if (string.IsNullOrEmpty(luisAppId) || string.IsNullOrEmpty(luisSubscriptionKey) || string.IsNullOrEmpty(ynabAccessToken) || string.IsNullOrEmpty(endpointUrl))
            {
                StartActivity(typeof(SettingsActivity));
                return;
            }

            var ynabBudget = await new YnabDataService(ynabAccessToken).GetBudgetsAsync();

            bottomSheetDialog = new BottomSheetDialog(this);
            bottomSheetDialog.SetContentView(Resource.Layout.dialog_ynab);

            var radioGroup = bottomSheetDialog.FindViewById<RadioGroup>(Resource.Id.budgets_radioGroup);

            if (ynabBudget is null)
                return;

            var budegtId = Preferences.Get(YNAB_SYNC_BUDGET_ID, null);

            foreach (var budget in ynabBudget.Data.Budgets)
            {
                var radioButton = new RadioButton(Platform.AppContext);
                radioButton.Id = 7756798 + new Random().Next(0, 599);
                radioButton.Text = budget.Name;
                radioButton.TextSize = 16;
                radioButton.SetTextColor(Color.White);
                radioButton.Checked = budegtId == budget.Id;

                radioGroup?.AddView(radioButton);

                radioButton.CheckedChange += (s, e) =>
                {
                    if (!e.IsChecked)
                        return;

                    Preferences.Set(YNAB_SYNC_BUDGET_ID, ynabBudget.Data.Budgets.FirstOrDefault(b => b.Name == radioButton.Text).Id);
                };
            }

            bottomSheetDialog.Show();
        }

        private async Task DisplayMessages()
        {
            messages = FilterMessagesBySenders();

            var transactions = await RemoteTransactionService.GetItemsAsync();
            
            foreach (var syncTransaction in transactions)
            {

                var transaction = new LocalTransaction(syncTransaction.MessageSender, syncTransaction.Date,
                    syncTransaction.TransactionType, syncTransaction.Amount, syncTransaction.TransactionCost,
                    syncTransaction.Code, syncTransaction.Principal, syncTransaction.MessageHash, syncTransaction.ThreadId, syncTransaction.Id,
                    false, true, syncTransaction.MessageHash);

                DisplayedTransactions.Add(transaction);
                transactionsAdapter?.NotifyItemInserted(DisplayedTransactions.Count - 1);
            }

            foreach (var message in messages)
            {
                var messageBodyHash = message.Body.ToHashedStringValue();
                var isInList = transactions.Any(t => t.MessageHash.Equals(messageBodyHash, StringComparison.Ordinal));

                if (isInList)
                    continue;

                var transaction = new LocalTransaction(message.Address, message.Date, null, null, null, null, null,
                    message.Body, message.ThreadId, message.Id, false, false, messageBodyHash);

                DisplayedTransactions.Add(transaction);
                transactionsAdapter?.NotifyItemInserted(DisplayedTransactions.Count - 1);
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
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
            
            await DisplayMessages();

            bottomSheetDialog?.Dismiss();

            UpdateStatusCard();
        }

        private string? GetTimeofDay()
        {
            var hour = DateTime.Now.Hour;
            string? greeting;
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

        public void OnItemClick(View itemView, int position, bool isLongClick)
        {
            var transaction = DisplayedTransactions[position];
            if (transaction.IsSelected)
            {
                if (isLongClick)
                    return;
                else
                {
                    DisplayedTransactions[position].IsSelected = false;
                    transactionsAdapter?.NotifyItemChanged(position);
                }
            }
            else
            {
                if (isLongClick)
                {
                    InSelectionMode = true;
                    DisplayedTransactions[position].IsSelected = true;
                    transactionsAdapter?.NotifyItemChanged(position);
                }

                else
                {
                    if(InSelectionMode)
                    {
                        DisplayedTransactions[position].IsSelected = true;
                        transactionsAdapter?.NotifyItemChanged(position);
                    }
                    return;
                }
            }

            InSelectionMode = DisplayedTransactions.Any(t => t.IsSelected);

            if (InSelectionMode)
                deleteTransactionImageview.Visibility = ViewStates.Visible;
            else
                deleteTransactionImageview.Visibility = ViewStates.Gone;
            
        }

        private async void DeleteTransactionImageview_Click(object sender, EventArgs e)
        {
            syncingProgressBar.Visibility = ViewStates.Visible;

            //Todo: handle deletion
            syncingProgressBar.Visibility = ViewStates.Gone;

        }
    }
}