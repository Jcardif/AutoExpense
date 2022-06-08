using System;
using System.Collections.Generic;
using System.Text;
using AutoExpense.Shared.Helpers;

namespace AutoExpense.Shared.Interfaces
{
    public interface ITransactionPrediction
    {
        public string ThreadId { get; set; }
        public string Id { get; set; }
        public long Date { get; set; }
        public string MessageSender { get; set; }
        public TransactionType? TransactionType { get; set; }
        public double? Amount { get; set; }
        public double? TransactionCost { get; set; }
        public string Code { get; set; }
        public string Principal { get; set; }
        public YnabSyncStatus YnabSyncStatus { get; set; }
    }
}
