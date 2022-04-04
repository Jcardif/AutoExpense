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
using AutoExpense.Android.Models;
using Uri=Android.Net.Uri;


namespace AutoExpense.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private RecyclerView transactionsRecyclerView;
        private TextView timeOfDayTextView, nameTextView, totalMessagesTextView, syncedMessagesTextView;
        

        private List<SMS> MoneyMessages { get; set; }

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

            timeOfDayTextView.Text=GetTimeofDay();

            RequestPermissions(new string[] { Manifest.Permission.ReadSms }, 0);

            var messages = GetAllSms();

            MoneyMessages = messages.Where(m => m.Address == "MPESA" || m.Address == "StanChart").ToList();
            var adapter = new TransactionsAdapter(MoneyMessages);
            transactionsRecyclerView.SetAdapter(adapter);


            totalMessagesTextView.Text = MoneyMessages.Count.ToString();
            syncedMessagesTextView.Text = "0";
            nameTextView.Text = "Josh N.";

        }

        private string? GetTimeofDay()
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
    }
}