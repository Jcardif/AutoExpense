using System;
using AutoExpense.Android.Models;

namespace AutoExpense.Android.Interfaces
{
    public interface ITransactionManager
    {
        public void OnTransactionSelected(LocalTransaction transaction);
    }
}