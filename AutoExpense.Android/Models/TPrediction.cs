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
    public class TPrediction
    {
        public TPrediction(string threadId, string id, long date, string messageSender, TransactionType? transactionType, float? amount, float? transactionCost, string code, string principal)
        {
            ThreadId = threadId;
            Id = id;
            Date = date;
            MessageSender = messageSender;
            TransactionType = transactionType;
            Amount = amount;
            TransactionCost = transactionCost;
            Code = code;
            Principal = principal;
        }

        public TPrediction()
        {
            
        }

        public string ThreadId { get; set; }
        public string Id { get; set; }
        public long Date { get; set; }
        public string MessageSender { get; set; }
        public TransactionType? TransactionType { get; set; }
        public float? Amount { get; set; }
        public float? TransactionCost { get; set; }
        public string Code { get; set; }
        public string Principal { get; set; }
    }
}