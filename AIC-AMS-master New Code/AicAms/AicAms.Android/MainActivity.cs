using System;
using AicAms.Azure;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Gcm.Client;
using Refractored.XamForms.PullToRefresh.Droid;
using Plugin.LocalNotifications;

namespace AicAms.Droid
{
    [Activity(Label = "AIC-AMS", Icon = "@drawable/icon", Theme = "@style/SplashScreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Locale, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;
            base.Window.RequestFeature(WindowFeatures.ActionBar);
            base.SetTheme(Resource.Style.MainTheme);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            //if (AicAms.Helpers.Settings.Locale != "en-US")
            //{
            //    var languageIso = "ar";
            //    var locale = new Java.Util.Locale(languageIso);// languageIso is locale string
            //    Java.Util.Locale.Default = locale;
            //    var config = new Android.Content.Res.Configuration { Locale = locale };
            //    BaseContext.Resources.UpdateConfiguration(config, BaseContext.Resources.DisplayMetrics);
            //}
            
            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(bundle);

            PullToRefreshLayoutRenderer.Init();
            XamEffects.Effects.Init();
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();
            LocalNotificationsImplementation.NotificationIconId = Resource.Drawable.icon;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            OnNewIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            if (intent.HasExtra("NotificationId"))
            {
                App.RequestSurpirse(intent.GetIntExtra("NotificationId", 0));
            }
        }
    }
}

