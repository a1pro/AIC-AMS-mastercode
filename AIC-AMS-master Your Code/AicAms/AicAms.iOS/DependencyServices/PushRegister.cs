using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AicAms.DependencyServices;
using AicAms.iOS.DependencyServices;
using AicAms.iOS.Helpers;
using Foundation;
using UIKit;

using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(PushRegister))]
namespace AicAms.iOS.DependencyServices
{
    public class PushRegister : IPushRegister
    {
        public void Register(string tag)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert |
                                                             UIRemoteNotificationType.Badge |
                                                             UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }

            Settings.IsAuthorized = true;
            Settings.PushTag = tag;
        }

        public void Unregister()
        {
            UIApplication.SharedApplication.UnregisterForRemoteNotifications();
            Settings.PushTag = string.Empty;
            Settings.IsAuthorized = false;
        }
    }
}