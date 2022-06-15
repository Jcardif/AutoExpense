using System;
using AutoExpense.Android.Models;
using AutoExpense.Shared.Helpers;

namespace AutoExpense.Android.Events
{
    public class TransactionServiceEventArgs : EventArgs
    {
        public TransactionServiceEventArgs(ListAction action, SyncTransaction item)
        {
            Action = action;
            Item = item;
        }

        /// <summary>
        /// The action that was performed
        /// </summary>
        public ListAction Action { get; }

        /// <summary>
        /// The item that was used.
        /// </summary>
        public SyncTransaction Item { get; }
    }
}