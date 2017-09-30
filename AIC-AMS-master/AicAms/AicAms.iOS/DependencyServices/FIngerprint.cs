using System;
using AicAms.DependencyServices;
using AicAms.iOS.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(Fingerprint))]
namespace AicAms.iOS.DependencyServices
{
    public class Fingerprint : IFingerprint
    {
        public void StartListen(Action success = null, Action failure = null)
        {
        }

        public void StopListen()
        {
        }
    }
}