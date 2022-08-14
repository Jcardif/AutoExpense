using System;
using AutoExpense.Android.Extensions;
using AutoExpense.Shared.Helpers;
using Microsoft.WindowsAzure.MobileServices;

namespace AutoExpense.Android.Models
{
	public class ToDoItem 
	{
        public ToDoItem()
        {
            Id = Guid.NewGuid().ToString().ToHashedStringValue().Substring(0,8);
            Name = "GENERIC";
            YnabSyncStatus = Enum.GetName(typeof(YnabSyncStatus), AutoExpense.Shared.Helpers.YnabSyncStatus.NotSynced);
            TransactionType = Enum.GetName(typeof(TransactionType), AutoExpense.Shared.Helpers.TransactionType.CashInflow);
           
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public string YnabSyncStatus { get; set; }
        public string TransactionType { get; set; }


        [UpdatedAt] public DateTimeOffset? UpdatedAt { get; set; }

        [Version] public string Version { get; set; }
        [Deleted] public string Deleted { get; set; }
    }
}

