using System.Globalization;
using AicAms.DependencyServices;
using AicAms.Resources;
using Xamarin.Forms;

namespace AicAms.Helpers
{
    public static class LocaleHelper
    {
        public static void Init()
        {
            //DependencyService.Get<ILocalize>().SetLocale(new CultureInfo(Settings.Locale));
            Settings.Culture = new CultureInfo(Settings.Locale);
            Resource.Culture = Settings.Culture;
        }

        public static async void ChangeCulture()
        {
            var res = await MessageViewer.Alert(Resource.ChangeLangMsg);
            if (res)
            {
                MessageViewer.Success(Resource.RestartApp);
                Settings.Locale = Settings.Locale == "en-US" ? "ar-SA" : "en-US";
            }
        }
    }
}