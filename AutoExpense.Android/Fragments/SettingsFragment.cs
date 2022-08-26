using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Views;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace AutoExpense.Android.Fragments
{
    public class SettingsFragment : Fragment
    {
        public override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_settings, container, false);
        }

        public override void OnViewCreated(View view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }
    }
}
