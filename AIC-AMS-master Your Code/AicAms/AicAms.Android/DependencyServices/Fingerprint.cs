using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Java.Security;
using Javax.Crypto;
using Android.Hardware.Fingerprints;
using Android.Support.V4.App;
using Android;
using System;
using AicAms.DependencyServices;
using AicAms.Droid.DependencyServices;
using Android.Security.Keystore;
using Android.Support.V4.Content;
using Xamarin.Forms;


[assembly: Dependency(typeof(Fingerprint))]
namespace AicAms.Droid.DependencyServices
{
    public class Fingerprint : IFingerprint
    {
        private KeyStore keyStore;

        private Cipher cipher;

        private string KEY_NAME = "AICAMS";

        private CancellationSignal cenCancellationSignal;

        public void StartListen(Action success = null, Action failure = null)
        {
            StopListen();
            var keyguardManager = (KeyguardManager)MainActivity.Instance.GetSystemService("keyguard");
            var fingerprintManager = (FingerprintManager)MainActivity.Instance.GetSystemService("fingerprint");

            if (ContextCompat.CheckSelfPermission(MainActivity.Instance, Manifest.Permission.UseFingerprint) != (int)Android.Content.PM.Permission.Granted)
                return;
            if (!fingerprintManager.IsHardwareDetected)
                Toast.MakeText(MainActivity.Instance, "Fingerprint authentication permission not enalbe", ToastLength.Long).Show();
            else
            {
                if (!fingerprintManager.HasEnrolledFingerprints)
                    Toast.MakeText(MainActivity.Instance, "Register at least one fingerprint in Settings", ToastLength.Long).Show();
                else
                {
                    if (!keyguardManager.IsKeyguardSecure)
                        Toast.MakeText(MainActivity.Instance, "Lock screen security not enable in Settings", ToastLength.Long).Show();
                    else
                        GenKey();
                    if (CipherInit())
                    {
                        FingerprintManager.CryptoObject cryptoObject = new FingerprintManager.CryptoObject(cipher);
                        var helper = new FingerprintHandler(success, failure);
                        cenCancellationSignal = helper.StartAuthentication(fingerprintManager, cryptoObject);
                    }
                }
            }
        }

        public void StopListen()
        {
            cenCancellationSignal?.Cancel();
        }

        private bool CipherInit()
        {
            try
            {
                cipher = Cipher.GetInstance(KeyProperties.KeyAlgorithmAes
                    + "/"
                    + KeyProperties.BlockModeCbc
                    + "/"
                    + KeyProperties.EncryptionPaddingPkcs7);
                keyStore.Load(null);
                IKey key = (IKey)keyStore.GetKey(KEY_NAME, null);
                cipher.Init(CipherMode.EncryptMode, key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void GenKey()
        {
            keyStore = KeyStore.GetInstance("AndroidKeyStore");
            KeyGenerator keyGenerator = null;
            keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
            keyStore.Load(null);
            keyGenerator.Init(new KeyGenParameterSpec.Builder(KEY_NAME, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeCbc)
                .SetUserAuthenticationRequired(true)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                .Build());
            keyGenerator.GenerateKey();
        }
    }


    internal class FingerprintHandler : FingerprintManager.AuthenticationCallback
    {
        private readonly Action _success;
        private readonly Action _failure;

        internal FingerprintHandler(Action success, Action failure)
        {
            _success = success;
            _failure = failure;
        }

        internal CancellationSignal StartAuthentication(FingerprintManager fingerprintManager, FingerprintManager.CryptoObject cryptoObject)
        {
            CancellationSignal cenCancellationSignal = new CancellationSignal();
            if (ContextCompat.CheckSelfPermission(MainActivity.Instance, Manifest.Permission.UseFingerprint) != (int)Android.Content.PM.Permission.Granted)
                return cenCancellationSignal;
            fingerprintManager.Authenticate(cryptoObject, cenCancellationSignal, 0, this, null);
            return cenCancellationSignal;
        }

        public override void OnAuthenticationFailed()
        {
            Toast.MakeText(MainActivity.Instance, "Fingerprint Authentication failed!", ToastLength.Long).Show();
            _failure?.Invoke();
        }

        public override void OnAuthenticationSucceeded(FingerprintManager.AuthenticationResult result)
        {
            Toast.MakeText(MainActivity.Instance, "Fingerprint Authentication passed!", ToastLength.Long).Show();
            _success?.Invoke();
        }
    }
}