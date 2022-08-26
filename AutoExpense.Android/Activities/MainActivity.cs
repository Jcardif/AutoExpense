using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AutoExpense.Android.Fragments;
using AutoExpense.Android.Helpers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace AutoExpense.Android.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        public Constants Constants { get; set; } = new Constants();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            
            AppCenter.Start(Constants.APP_CENTER_SECRET, typeof(Analytics), typeof(Crashes));

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // Request all relevant permissions
            // todo handle when permission is denied
            RequestPermissions(new[] { Manifest.Permission.ReadSms }, 0);

            if (savedInstanceState is null)
            {
                SupportFragmentManager
                    .BeginTransaction()
                    .SetReorderingAllowed(true)
                    .Add(Resource.Id.fragment_container_view, new HomeFragment(), null)
                    .Commit();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}