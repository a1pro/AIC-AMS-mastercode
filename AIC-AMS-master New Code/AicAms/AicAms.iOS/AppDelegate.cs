using System;
using System.Collections.Generic;
using System.Linq;
using WindowsAzure.Messaging;
using AicAms.Azure;
using AicAms.iOS.Helpers;
using Foundation;
using UIKit;
using XFShapeView.iOS;
using UserNotifications;

namespace AicAms.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private SBNotificationHub Hub { get; set; }

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            XamEffects.iOS.Effects.Init();
            ShapeRenderer.Init();
            OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                        (approved, error) => { });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            return base.FinishedLaunching(app, options);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            Hub = new SBNotificationHub(AzureConstants.ListenConnectionString, AzureConstants.NotificationHubName);

            Hub.UnregisterAllAsync(deviceToken, (error) =>
            {
                if (error != null)
                {
                    Console.WriteLine("Error calling Unregister: {0}", error.ToString());
                    return;
                }

                NSSet tags = new NSSet(Settings.PushTag); // create tags if you want
                Hub.RegisterNativeAsync(deviceToken, tags, (errorCallback) =>
                {
                    if (errorCallback != null)
                        Console.WriteLine("RegisterNativeAsync error: " + errorCallback.ToString());
                });
            });
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            //Get the aps dictionary
            var aps = userInfo.ObjectForKey(new NSString("aps")) as NSDictionary;
            if (aps == null)
                return;

            var rid = 0;

            if (aps.ContainsKey(new NSString("alert")) && aps.ContainsKey(new NSString("rid")))
            {
                var nsString = aps[new NSString("rid")] as NSString;
                if (nsString != null)
                {
                    int.TryParse(nsString.ToString(), out rid);
                }
            }

            if (rid > 0)
                App.RequestSurpirse(rid);
        }
    }
}
