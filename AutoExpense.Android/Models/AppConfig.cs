using System.ComponentModel.DataAnnotations;

namespace AutoExpense.Android.Models
{
    public class AppConfig
    {
        [Display(Name = "Luis App Id"), Required(AllowEmptyStrings = false, ErrorMessage = "Luis App Id needed for predictions")]
        public string LuisAppId { get; set; }

        [Display(Name ="Luis Subscription Key"), Required(AllowEmptyStrings = false, ErrorMessage = "Luis Subscription key needed for predictions")]
        public string LuisSubscriptionKey { get; set; }

        [Display(Name = "Endpoint Url"), Required(AllowEmptyStrings = false, ErrorMessage = "Endpoint Url needed for predictions")]
        public string EndPointUrl { get; set; }

        [Display(Name = "YNAB Access Token"), Required(AllowEmptyStrings = false, ErrorMessage = "Access Token Needed to save transaction to YNAB")]
        public string YnabAccessToken { get; set; }

        [Display(Name = "Save to YNAB")]
        public bool SaveToYnab { get; set; }

    }
}