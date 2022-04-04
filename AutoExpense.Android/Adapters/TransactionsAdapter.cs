using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Icu.Text;
using AndroidX.RecyclerView.Widget;
using AutoExpense.Android.Models;

namespace AutoExpense.Android.Adapters
{
    public class TransactionsAdapter : RecyclerView.Adapter
    {
        private readonly List<SMS> _messages;

        public TransactionsAdapter(List<SMS> messages)
        {
            _messages = messages;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is TransactionsViewHolder vh)
            {
                vh.AddressTextView.Text = _messages[position].Address;
                vh.DateTimeTextView.Text = new SimpleDateFormat("MMM dd, HH:mm").Format(_messages[position].Date);

                // todo : handle sync and amt
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.item_transaction, parent, false);

            var addressTextView = view?.FindViewById<TextView>(Resource.Id.sender_textView);
            var dateTimeText = view?.FindViewById<TextView>(Resource.Id.date_time_textView);
            var amountTextView = view?.FindViewById<TextView>(Resource.Id.amount);
            var syncProblem = view?.FindViewById<ImageView>(Resource.Id.sync_problem_imageView);

            var holder = new TransactionsViewHolder(view, addressTextView, dateTimeText);
            holder.SyncProblem = syncProblem;
            holder.AmountTextView = amountTextView;
            return holder;
        }

        public override int ItemCount => _messages.Count;
    }


    public class TransactionsViewHolder : RecyclerView.ViewHolder
    {
        public TransactionsViewHolder(View itemView, TextView addressTextView, TextView dateTimeTextView) : base(itemView)
        {
            View = itemView;
            AddressTextView = addressTextView;
            DateTimeTextView = dateTimeTextView;
        }

        public View View { get; set; }
        public TextView AddressTextView { get; set; }
        public TextView DateTimeTextView { get; set; }
        public TextView AmountTextView { get; set; }
        public ImageView SyncProblem { get; set; }

    }
}