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

namespace AutoExpense.Android.Interfaces
{
    public interface ISendersManager
    {
        public void SenderSelected(string sender);
        public void SenderRemoved(string sender);
    }
}