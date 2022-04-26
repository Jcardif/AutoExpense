using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoExpense.Android.Models
{
    public class YnabTransaction
    {
        public YnabTransaction(Transaction transaction)
        {
            Transaction = transaction;
        }

        [JsonProperty("transaction")]
        public Transaction Transaction { get; set; }
    }

    public class Transaction
    {
        public Transaction(string accountId, string date, int amount, string payeeId, string payeeName, string categoryId,
            string memo, string cleared, bool approved, string flagColor, string importId, List<Subtransaction> subtransactions)
        {
            AccountId = accountId;
            Date = date;
            Amount = amount;
            PayeeId = payeeId;
            PayeeName = payeeName;
            CategoryId = categoryId;
            Memo = memo;
            Cleared = cleared;
            Approved = approved;
            FlagColor = flagColor;
            ImportId = importId;
            Subtransactions = subtransactions;
        }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("payee_id")]
        public string PayeeId { get; set; }

        [JsonProperty("payee_name")]
        public string PayeeName { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("cleared")]
        public string Cleared { get; set; }

        [JsonProperty("approved")]
        public bool Approved { get; set; }

        [JsonProperty("flag_color")]
        public string FlagColor { get; set; }

        [JsonProperty("import_id")]
        public string ImportId { get; set; }

        [JsonProperty("subtransactions")]
        public List<Subtransaction> Subtransactions { get; set; }
    }

    public partial class Subtransaction
    {
        public Subtransaction(int amount, string payeeId, string payeeName, string categoryId, string memo)
        {
            Amount = amount;
            PayeeId = payeeId;
            PayeeName = payeeName;
            CategoryId = categoryId;
            Memo = memo;
        }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("payee_id")]
        public string PayeeId { get; set; }

        [JsonProperty("payee_name")]
        public string PayeeName { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; }
    }
}

