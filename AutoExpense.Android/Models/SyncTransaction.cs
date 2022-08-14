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
using AutoExpense.Shared.Helpers;
using Microsoft.WindowsAzure.MobileServices;

namespace AutoExpense.Android.Models
{
    /// <summary>
    /// The model that is for the Transaction pulled from the service.  This must
    /// match what is coming from the service.
    /// </summary>
    public class SyncTransaction
    {
        public SyncTransaction(string threadId, long date, string messageSender, TransactionType? transactionType,
            double? amount, double? transactionCost, string code, string principal, YnabSyncStatus ynabSyncStatus,
            string messageHash)
        {
            ThreadId = threadId;
            Date = date;
            MessageSender = messageSender;
            TransactionType = transactionType;
            Amount = amount;
            TransactionCost = transactionCost;
            Code = code;
            Principal = principal;
            YnabSyncStatus = ynabSyncStatus;
            MessageHash = messageHash;
        }

        public SyncTransaction()
        {

        }

        public string Id { get; set; }
        public string ThreadId { get; set; }
        public long Date { get; set; }
        public string MessageSender { get; set; }
        public TransactionType? TransactionType { get; set; }
        public double? Amount { get; set; }
        public double? TransactionCost { get; set; }
        public string Code { get; set; }
        public string Principal { get; set; }
        public YnabSyncStatus YnabSyncStatus { get; set; }
        public string MessageHash { get; set; }

        [UpdatedAt] public DateTimeOffset? UpdatedAt { get; set; }

        [Version] public string Version { get; set; }
        [Deleted] public string Deleted { get; set; }
    }
}