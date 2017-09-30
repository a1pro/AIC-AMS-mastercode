using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace AicAms.Droid.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static string PushTag
        {
            get { return AppSettings.GetValueOrDefault<string>("__android_pushTag"); }
            set { AppSettings.AddOrUpdateValue("__android_pushTag", value); }
        }

        public static bool IsAuthorized
        {
            get { return AppSettings.GetValueOrDefault<bool>("__android_isAuthorized"); }
            set { AppSettings.AddOrUpdateValue("__android_isAuthorized", value); }
        }

    }
}