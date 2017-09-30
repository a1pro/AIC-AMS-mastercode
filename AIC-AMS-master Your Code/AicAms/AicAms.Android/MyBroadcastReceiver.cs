using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using WindowsAzure.Messaging;
using AicAms.Azure;
using AicAms.Droid.Helpers;
using AicAms.Models.Auth;
using Android.Media;
using Android.Support.V4.App;

[assembly: Permission(Name = "com.aicams.android.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.aicams.android.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is needed only for Android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
namespace AicAms.Droid
{
    [BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE },
    Categories = new string[] { "com.aicams.android" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
    Categories = new string[] { "com.aicams.android" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
    Categories = new string[] { "com.aicams.android" })]
    public class MyBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        public static string[] SENDER_IDS { get; set; } = new string[] { AzureConstants.AndroidSenderID };
    }

    [Service] // Must use the service tag
    public class PushHandlerService : GcmServiceBase
    {
        public static string RegistrationID { get; private set; }

        public static NotificationHub Hub { get; private set; }

        public PushHandlerService() : base(AzureConstants.AndroidSenderID)
        {
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            var msg = new StringBuilder();

            if (intent?.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                    msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
            }

            var title = intent?.Extras?.GetString("title") ?? string.Empty;
            var message = intent?.Extras?.GetString("message") ?? string.Empty;
            var rid = intent?.Extras?.GetString("rid") ?? string.Empty;

            int id;
            int.TryParse(rid, out id);

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(title))
            {
                if (id != 0 && Settings.IsAuthorized)
                    CreateNotification(id, title, message);
            }
        }

        protected override void OnError(Context context, string errorId)
        {
        }

        protected override void OnRegistered(Context context, string registrationId)
        {
            RegistrationID = registrationId;

            Hub = new NotificationHub(AzureConstants.NotificationHubName, AzureConstants.ListenConnectionString, context);

            try
            {
                Hub.Unregister();
            }
            catch (Exception ex)
            {
            }

            try
            {
                var hubRegistration = Hub.Register(registrationId, Settings.PushTag);
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
        }

        private void CreateNotification(int id, string title, string desc)
        {
            //Create notification
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            //Create an intent to show ui
            var uiIntent = new Intent(this, typeof(MainActivity));
            uiIntent.PutExtra("NotificationId", id);

            //Use Notification Builder
            var builder = new NotificationCompat.Builder(this);

            //Create the notification
            //we use the pending intent, passing our ui intent over which will get called
            //when the notification is tapped.
            var notification = builder.SetContentIntent(PendingIntent.GetActivity(this, 0, uiIntent, PendingIntentFlags.UpdateCurrent))
                    .SetSmallIcon(Android.Resource.Drawable.SymActionEmail)
                    .SetTicker(title)
                    .SetContentTitle(title)
                    .SetContentText(desc)
                    //Set the notification sound
                    .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                    //Auto cancel will remove the notification once the user touches it
                    .SetAutoCancel(true).Build();

            //Show the notification
            notificationManager?.Notify(id, notification);
        }
    }
}