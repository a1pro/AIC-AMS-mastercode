using System;
using AicAms.Azure;
using AicAms.DependencyServices;
using AicAms.Droid.DependencyServices;
using AicAms.Droid.Helpers;
using Gcm.Client;
using Xamarin.Forms;

[assembly: Dependency(typeof(PushRegister))]
namespace AicAms.Droid.DependencyServices
{
    public class PushRegister : IPushRegister
    {
        public void Register(string tag)
        {
            // Check to ensure everything's set up right
            GcmClient.CheckDevice(MainActivity.Instance);
            GcmClient.CheckManifest(MainActivity.Instance);

            // Register for push notifications
            GcmClient.Register(MainActivity.Instance, AzureConstants.AndroidSenderID);
            Settings.IsAuthorized = true;
            Settings.PushTag = tag;
        }

        public void Unregister()
        {
            Settings.PushTag = string.Empty;
            GcmClient.UnRegister(MainActivity.Instance);
            try
            {
                PushHandlerService.Hub?.Unregister();
            }
            catch (Exception e)
            {
            }
            Settings.IsAuthorized = false;

        }
    }
}