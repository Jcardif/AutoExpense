using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoExpense.Android.Models;
using Newtonsoft.Json;
using static AutoExpense.Android.Helpers.Constants;

namespace AutoExpense.Android.Services
{
	public class YnabDataService
	{
        private readonly HttpClient client;

        public YnabDataService(string accessToken)
		{
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<bool> SaveTransactionAsync(YnabTransaction transaction, string budgetId)
        {
            var json = JsonConvert.SerializeObject(transaction);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{YNAB_BASE_URL}/budgets/{budgetId}/transactions", content);

            if (response.StatusCode == HttpStatusCode.Created)
                return true;

            else if (response.StatusCode == HttpStatusCode.Conflict)
                return true;

            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return false;

            else
                return false;
        }

        public async Task<YnabBudget?> GetBudgetsAsync()
        {
            var response = await client.GetAsync($"{YNAB_BASE_URL}/budgets");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                var ynabBudget = JsonConvert.DeserializeObject<YnabBudget>(json);
                return ynabBudget;
            }
            else
                return null;
        }
	}
}

