using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoExpense.Android.Helpers;
using AutoExpense.Android.Models;
using Newtonsoft.Json;

namespace AutoExpense.Android.Services
{
    public class LuisPredictionService
    {
        private readonly string _appId;
        private readonly string _key;
        private readonly string _endpoint;
        private readonly HttpClient _client;

        public LuisPredictionService(string appId, string key, string endpoint)
        {
            _appId = appId;
            _key = key;
            _endpoint = endpoint;

        }

        public async Task<LuisPrediction?> GetPrediction(string query)
        {
            var url =
                $"{_endpoint}/luis/prediction/v3.0/apps/{_appId}/slots/production/predict?verbose=true&show-all-intents=true&log=true&subscription-key={_key}&query={query}";

            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LuisPrediction>(json);

        }
    }
}