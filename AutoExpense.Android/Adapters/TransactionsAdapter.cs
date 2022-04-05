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
using Android.Graphics;
using Android.Icu.Text;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using AutoExpense.Android.Models;
using Xamarin.Essentials;

namespace AutoExpense.Android.Adapters
{
    public class TransactionsAdapter : RecyclerView.Adapter
    {
        private readonly List<Transaction> _transactions;

        public TransactionsAdapter(List<Transaction> transactions)
        {
            _transactions = transactions;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is TransactionsViewHolder vh)
            {
                vh.AddressTextView.Text = _transactions[position].MessageSender;
                vh.DateTimeTextView.Text = new SimpleDateFormat("MMM dd, HH:mm").Format(_transactions[position].Date);

                if (_transactions[position].TransactionType==TransactionType.Fuliza || _transactions[position].TransactionType == TransactionType.CashOutflow)
                {
                    vh.AmountTextView.Text = $"- Ksh {_transactions[position].Amount}";
                    vh.AmountTextView.Visibility = ViewStates.Visible;
                    vh.AmountTextView.SetTextColor(new Color(ContextCompat.GetColor(Platform.AppContext, Resource.Color.colorPink)));
                    vh.SyncProblem.Visibility = ViewStates.Invisible;
                }
                else if (_transactions[position].TransactionType==TransactionType.CashInflow)
                {
                    vh.AmountTextView.Text = $"Ksh {_transactions[position].Amount}";
                    vh.AmountTextView.Visibility = ViewStates.Visible;
                    vh.AmountTextView.SetTextColor(new Color(ContextCompat.GetColor(Platform.AppContext, Resource.Color.colorGreen)));
                    vh.SyncProblem.Visibility = ViewStates.Invisible;
                }
                else if (_transactions[position].Amount is null)
                {
                    vh.AmountTextView.Visibility = ViewStates.Invisible;
                    vh.SyncProblem.Visibility = ViewStates.Visible;
                }
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

        public override int ItemCount => _transactions.Count;
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