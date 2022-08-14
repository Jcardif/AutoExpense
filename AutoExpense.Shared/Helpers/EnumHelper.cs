using System;
using System.Collections.Generic;
using System.Text;

namespace AutoExpense.Shared.Helpers
{
    public enum YnabSyncStatus
    {
        Skipped,
        Synced,
        NotSynced
    }

    public enum TransactionType
    {
        CashOutflow,
        CashInflow,
        Fuliza

    }

    /// <summary>
    /// The list of possible actions.
    /// </summary>
    public enum ListAction { Add, Delete, Update };
}
