using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AicAms.DependencyServices;
using AicAms.iOS.DependencyServices;
using CoreLocation;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AskCapabilites))]
namespace AicAms.iOS.DependencyServices
{
    public class AskCapabilites : IAskCapabilites
    {
        public bool AskGps()
        {
            if (CLLocationManager.Status == CLAuthorizationStatus.Denied)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    NSString settingsString = UIApplication.OpenSettingsUrlString;
                    NSUrl url = new NSUrl(settingsString);
                    UIApplication.SharedApplication.OpenUrl(url);
                }
                return false;
            }
            return true;
        }
    }
}