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
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AutoExpense.Android.Adapters;
using AutoExpense.Android.Interfaces;
using AutoExpense.Android.Models;
using Google.Android.Material.BottomSheet;
using Xamarin.Essentials;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using Uri=Android.Net.Uri;


namespace AutoExpense.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISendersManager
    {
        private RecyclerView transactionsRecyclerView;
        private TextView timeOfDayTextView, nameTextView, totalMessagesTextView, syncedMessagesTextView;
        private Button sendersButton, syncButton;
        private TransactionsAdapter transactionsAdapter;
        private BottomSheetDialog bottomSheetDialog;
        public List<SMS> SelectMessages { get; set; } = new List<SMS>();
        public List<SMS> messages { get; set; }
        public List<string> senders { get; set; }

        private const string SENDERS_LIST = "SelectSenders";
        public List<string> SelectSenders { get; set; }=new List<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            transactionsRecyclerView = FindViewById<RecyclerView>(Resource.Id.transactions_recyclerView);
            timeOfDayTextView = FindViewById<TextView>(Resource.Id.time_of_day_textView);
            nameTextView = FindViewById<TextView>(Resource.Id.name_textView);
            totalMessagesTextView = FindViewById<TextView>(Resource.Id.total_messages_textView);
            syncedMessagesTextView = FindViewById<TextView>(Resource.Id.synced_messages_textView);
            sendersButton = FindViewById<Button>(Resource.Id.senders_button);
            syncButton = FindViewById<Button>(Resource.Id.sync_button);

            timeOfDayTextView.Text=GetTimeofDay();

            RequestPermissions(new string[] { Manifest.Permission.ReadSms }, 0);

            LoadMessages();

            transactionsAdapter = new TransactionsAdapter(SelectMessages);
            transactionsRecyclerView.SetAdapter(transactionsAdapter);


            UpdateStatusCard(SelectMessages);
            nameTextView.Text = "Josh N.";

            sendersButton.Click += SendersButton_Click;
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
                    SelectMessages.Add(message);
            }

        }

        private void UpdateStatusCard(List<SMS> selectMessages)
        {
            totalMessagesTextView.Text = SelectMessages.Count.ToString();
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

            SelectMessages.Clear();

            foreach (var message in messages)
            {
                if(SelectSenders.Contains(message.Address))
                    SelectMessages.Add(message);
            }

            transactionsAdapter.NotifyDataSetChanged();
            bottomSheetDialog.Dismiss();

            UpdateStatusCard(SelectMessages);
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