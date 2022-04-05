using Android.App;
using Android.Media.Audiofx;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AutoExpense.Android.Extensions;
using AutoExpense.Android.Models;
using Syncfusion.Android.DataForm;
using Xamarin.Essentials;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using static AutoExpense.Android.Helpers.Constants;

namespace AutoExpense.Android.Activities
{
    [Activity(Label = "SettingsActivity", Theme = "@style/AppTheme", MainLauncher = false)]
    public class SettingsActivity : AppCompatActivity, View.IOnClickListener
    {
        private SfDataForm configDataForm;
        private Toolbar settingsToolbar;
        private Button saveButton;
        public AppConfig AppConfig { get; set; }

        public SettingsActivity()
        {
            var luisAppId = Preferences.Get(LUIS_APP_ID, null);
            var luisSubscriptionKey = Preferences.Get(LUIS_SUBBSCRIPTION_KEY, null);
            var ynabAccessToken = Preferences.Get(YNAB_ACCESS_TOKEN, null);
            var endpointUrl = Preferences.Get(ENDPOINT_URL, null);

            AppConfig = new AppConfig
            {
                LuisAppId = luisAppId,
                LuisSubscriptionKey = luisSubscriptionKey,
                YnabAccessToken = ynabAccessToken,
                EndPointUrl = endpointUrl
            };
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.fragment_settings);
            configDataForm = FindViewById<SfDataForm>(Resource.Id.sf_data_form);
            saveButton = FindViewById<Button>(Resource.Id.save_settings_button);
            settingsToolbar = FindViewById<Toolbar>(Resource.Id.settings_toolbar);

            SetUpToolBar();

            configDataForm.DataObject = AppConfig;
            configDataForm.LayoutManager = new DataFormLayoutManagerExt(configDataForm);
            configDataForm.LabelPosition = LabelPosition.Top;
            configDataForm.ValidationMode = ValidationMode.LostFocus;
            configDataForm.CommitMode = CommitMode.LostFocus;
            configDataForm.ColumnCount = 1;

            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            configDataForm.Commit();

            if (string.IsNullOrEmpty(AppConfig.LuisAppId) || string.IsNullOrEmpty(AppConfig.LuisSubscriptionKey) ||
                string.IsNullOrEmpty(AppConfig.YnabAccessToken) || string.IsNullOrEmpty(AppConfig.EndPointUrl))
            {
                Toast.MakeText(Platform.AppContext, "Settings Values Cannot be empty", ToastLength.Short)?.Show();
                return;
            }

            Preferences.Set(LUIS_APP_ID, AppConfig.LuisAppId);
            Preferences.Set(LUIS_SUBBSCRIPTION_KEY, AppConfig.LuisSubscriptionKey);
            Preferences.Set(YNAB_ACCESS_TOKEN, AppConfig.YnabAccessToken);
            Preferences.Set(ENDPOINT_URL, AppConfig.EndPointUrl);

            Toast.MakeText(Platform.AppContext, "Settings Saved", ToastLength.Short)?.Show();

            Platform.CurrentActivity.OnBackPressed();
        }

        private void SetUpToolBar()
        {
            var activity = ((AppCompatActivity)Platform.CurrentActivity);

            activity.SetSupportActionBar(settingsToolbar);
            activity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            activity.SupportActionBar.SetDisplayShowHomeEnabled(true);
            activity.SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_baseline_arrow_back_24);

            settingsToolbar?.SetNavigationOnClickListener(this);
        }

        public void OnClick(View? v)
        {
            Platform.CurrentActivity.OnBackPressed();
        }
    }
}