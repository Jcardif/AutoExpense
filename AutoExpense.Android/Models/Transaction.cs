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

namespace AutoExpense.Android.Models
{
    public class Transaction
    {
        public Transaction(string messageSender, long date, TransactionType? transactionType, float? amount, float? transactionCost, string code, string principal, string body)
        {
            MessageSender = messageSender;
            Date = date;
            TransactionType = transactionType;
            Amount = amount;
            TransactionCost = transactionCost;
            Code = code;
            Principal = principal;
            Body = body;
        }

        public string MessageSender { get; set; }
        public long Date { get; set; }
        public TransactionType? TransactionType { get; set; }
        public float? Amount { get; set; }
        public float? TransactionCost { get; set; }
        public string Code { get; set; }
        public string Principal { get; set; }
        public string Body { get; set; }

    }

    public enum TransactionType
    {
        CashOutflow,
        CashInflow,
        Fuliza

    }
}
