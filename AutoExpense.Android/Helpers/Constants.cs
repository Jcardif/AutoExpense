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

namespace AutoExpense.Android.Helpers
{
    public static class Constants
    {
        public static string SENDERS_LIST = "SelectSenders";
        public static string LUIS_APP_ID = "LuisAppId";
        public static string LUIS_SUBBSCRIPTION_KEY = "LuisSubscriptionKey";
        public static string YNAB_ACCESS_TOKEN = "YnabAccessToken";
        public static string ENDPOINT_URL = "EndPointUrl";
        public static string SAVE_TO_YNAB = "save_to_ynab";
        public static string FIREBASE_DATABASE_URL = "https://autoexpense-legytt-2022-default-rtdb.firebaseio.com/";
        public static string YNAB_BASE_URL = "https://api.youneedabudget.com/v1/";

        public static string TPREDICTION_CHILD_NAME = "TPrediction";
        public static string LUIS_APP_CONFIG_CHILD_NAME = "LuisConfig";

        public static string YNAB_SYNC_BUDGET_ID="YnabSyncBudgetId";

        /// <summary>
        /// The base URI for the DataSync service.
        /// </summary>
        public static string BackendUrl = "https://autoexpense.azurewebsites.net";
    }
}