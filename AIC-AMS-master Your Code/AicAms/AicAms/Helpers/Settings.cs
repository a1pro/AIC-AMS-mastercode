using System;
using System.Globalization;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace AicAms.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static string Locale
        {
            get { return AppSettings.GetValueOrDefault("Culture", "en-US"); }
            set
            {
                AppSettings.AddOrUpdateValue("Culture", value);
            }
        }

        private static CultureInfo _culture;

        public static CultureInfo Culture
        {
            get { return _culture == null ? _culture = new CultureInfo(Locale) : _culture; }
            set { _culture = value; }
        }
    }
}