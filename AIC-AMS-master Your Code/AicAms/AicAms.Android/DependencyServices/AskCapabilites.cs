using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AicAms.DependencyServices;
using AicAms.Droid.DependencyServices;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

[assembly: Dependency(typeof(AskCapabilites))]
namespace AicAms.Droid.DependencyServices
{
    public class AskCapabilites : IAskCapabilites
    {
        public bool AskGps()
        {
            var locationManager = (LocationManager)Forms.Context.GetSystemService(Context.LocationService);

            if (!locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                var gpsSettingIntent = new Intent(Settings.ActionLocationSourceSettings);
                Forms.Context.StartActivity(gpsSettingIntent);
                return false;
            }
            return true;
        }
    }
}