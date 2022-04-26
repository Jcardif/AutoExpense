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
    public class LocalTransaction
    {
        public LocalTransaction(string messageSender, long date, TransactionType? transactionType, double? amount, double? transactionCost, string code, string principal, string body, string threadId, string id)
        {
            MessageSender = messageSender;
            Date = date;
            TransactionType = transactionType;
            Amount = amount;
            TransactionCost = transactionCost;
            Code = code;
            Principal = principal;
            Body = body;
            ThreadId = threadId;
            Id = id;
        }

        public string MessageSender { get; set; }
        public long Date { get; set; }
        public TransactionType? TransactionType { get; set; }
        public double? Amount { get; set; }
        public double? TransactionCost { get; set; }
        public string Code { get; set; }
        public string Principal { get; set; }
        public string Body { get; set; }
        public string ThreadId { get; set; }
        public string Id { get; set; }

    }

    public enum TransactionType
    {
        CashOutflow,
        CashInflow,
        Fuliza

    }
}
