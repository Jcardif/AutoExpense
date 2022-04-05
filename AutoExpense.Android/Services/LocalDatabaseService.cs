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
using AutoExpense.Android.Models;
using SQLite;

namespace AutoExpense.Android.Services
{
    public class LocalDatabaseService
    {
        public SQLiteConnection DatabaseConnection { get; set; }
        public LocalDatabaseService(string dbPath)
        {
            DatabaseConnection = new SQLiteConnection(dbPath);
            InitTables();
        }

        private void InitTables()
        {
            DatabaseConnection.CreateTable<TPrediction>();
        }

        public void SaveTransactionPrediction(TPrediction tPrediction) => DatabaseConnection.Insert(tPrediction);
        public void DeleteTransactionPrediction(TPrediction tPrediction) => DatabaseConnection.Delete(tPrediction);
        public List<TPrediction> GetTransactionPredictions() => DatabaseConnection.Table<TPrediction>().ToList();
    }
}