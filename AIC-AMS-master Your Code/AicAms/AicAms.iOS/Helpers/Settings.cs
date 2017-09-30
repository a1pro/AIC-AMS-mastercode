using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace AicAms.iOS.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static string PushTag
        {
            get { return AppSettings.GetValueOrDefault<string>("__ios_pushTag"); }
            set { AppSettings.AddOrUpdateValue("__ios_pushTag", value); }
        }

        public static bool IsAuthorized
        {
            get { return AppSettings.GetValueOrDefault<bool>("__ios_isAuthorized"); }
            set { AppSettings.AddOrUpdateValue("__ios_isAuthorized", value); }
        }

    }
}