using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoExpense.Android.Models
{
    public partial class YnabBudget
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("budgets")]
        public List<Budget> Budgets { get; set; }

        [JsonProperty("default_budget")]
        public Budget DefaultBudget { get; set; }
    }

    public partial class Budget
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("last_modified_on")]
        public DateTimeOffset LastModifiedOn { get; set; }

        [JsonProperty("first_month")]
        public string FirstMonth { get; set; }

        [JsonProperty("last_month")]
        public string LastMonth { get; set; }

        [JsonProperty("date_format")]
        public DateFormat DateFormat { get; set; }

        [JsonProperty("currency_format")]
        public CurrencyFormat CurrencyFormat { get; set; }

        [JsonProperty("accounts")]
        public List<Account> Accounts { get; set; }
    }

    public partial class Account
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("on_budget")]
        public bool OnBudget { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("balance")]
        public long Balance { get; set; }

        [JsonProperty("cleared_balance")]
        public long ClearedBalance { get; set; }

        [JsonProperty("uncleared_balance")]
        public long UnclearedBalance { get; set; }

        [JsonProperty("transfer_payee_id")]
        public string TransferPayeeId { get; set; }

        [JsonProperty("direct_import_linked")]
        public bool DirectImportLinked { get; set; }

        [JsonProperty("direct_import_in_error")]
        public bool DirectImportInError { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }

    public partial class CurrencyFormat
    {
        [JsonProperty("iso_code")]
        public string IsoCode { get; set; }

        [JsonProperty("example_format")]
        public string ExampleFormat { get; set; }

        [JsonProperty("decimal_digits")]
        public long DecimalDigits { get; set; }

        [JsonProperty("decimal_separator")]
        public string DecimalSeparator { get; set; }

        [JsonProperty("symbol_first")]
        public bool SymbolFirst { get; set; }

        [JsonProperty("group_separator")]
        public string GroupSeparator { get; set; }

        [JsonProperty("currency_symbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("display_symbol")]
        public bool DisplaySymbol { get; set; }
    }

    public partial class DateFormat
    {
        [JsonProperty("format")]
        public string Format { get; set; }
    }
}
