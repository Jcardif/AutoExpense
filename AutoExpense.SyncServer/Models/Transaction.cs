using AutoExpense.Shared.Helpers;
using AutoExpense.Shared.Interfaces;
using Microsoft.AspNetCore.Datasync.EFCore;

namespace AutoExpense.SyncServer.Models
{
    public class Transaction : EntityTableData, ITransactionPrediction

    {
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
    }
}
