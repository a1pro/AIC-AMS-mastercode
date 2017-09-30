using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AicAms.ApiFactory;
using AicAms.DependencyServices;
using AicAms.Helpers;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Details;
using Plugin.Connectivity;
using Plugin.Fingerprint;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace AicAms.ViewModels.Auth
{
    public class LoginViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly AuthFactory _factory;

        private User _user;

        private IFingerprint _fingerprint = DependencyService.Get<IFingerprint>();

        private bool _visibleButton;

        public bool VisibleButton
        {
            get { return _visibleButton; }
            set { SetProperty(ref _visibleButton, value); }
        }

        private bool _visibleIndicator;

        public bool VisibleIndicator
        {
            get { return _visibleIndicator; }
            set { SetProperty(ref _visibleIndicator, value); }
        }

        private string _login;

        public string Login
        {
            get { return _login; }
            set { SetProperty(ref _login, value); }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private bool _visibleFingerprint;

        public bool VisibleFingerprint
        {
            get { return _visibleFingerprint; }
            set { SetProperty(ref _visibleFingerprint, value); }
        }

        public ICommand AuthCommand { get; }

        public ICommand FingerprintAuthCommand { get; }

        public ICommand ChangeLangCommand { get; }

        public LoginViewModel()
        {
            _factory = new AuthFactory();
            VisibleButton = true;
            VisibleIndicator = false;

            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user != null)
            {
                Login = _user.Login;
                var vis = CrossFingerprint.Current.IsAvailableAsync();
                vis.Wait();
                VisibleFingerprint = vis.Result;
            }

            AuthCommand = new Command(Auth);
            FingerprintAuthCommand = new Command(FingerprintAuth);
            ChangeLangCommand = new Command(LocaleHelper.ChangeCulture);

            if (Device.RuntimePlatform == Device.Android && VisibleFingerprint)
            {
                _fingerprint.StartListen(FingerSuccess);
            }
        }

        private async void Auth()
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                MessageViewer.Error(Resource.NotEnouthFields);
                return;
            }

            VisibleButton = false;
            VisibleIndicator = true;
            var user = await _factory.SignIn(Login, Password, Device.RuntimePlatform == Device.Android, _cancellationToken.Token);
            if (user.ResultCode == ResultCode.Success)
            {
                App.Realm.Write(() =>
                {
                    App.Realm.RemoveAll<User>();
                    App.Realm.Add(user.Data);
                });
                _user = App.Realm.All<User>().FirstOrDefault();
                FingerSuccess();
            }
            VisibleButton = true;
            VisibleIndicator = false;
        }

        public void FingerSuccess()
        {
            Device.BeginInvokeOnMainThread(delegate
            {
                DependencyService.Get<IPushRegister>().Register(_user.Token);
            });
            NavigationService.InitMsPage();
            NavigationService.SetDetailPage(new DashboardViewModel(), SelectedMenuOptions.Dashboard);
            MessagingCenter.Send(this, "Authed");
            _fingerprint.StopListen();
        }

        private async void FingerprintAuth()
        {
            if (Device.RuntimePlatform == Device.Android)
                return;

            if (!CrossConnectivity.Current.IsConnected)
            {
                await MessageViewer.ErrorAsync(Resource.CheckInternetMsg);
                return;
            }

            var result = await CrossFingerprint.Current.AuthenticateAsync(Resource.FingerprintComponentMsg);
            if (result.Authenticated)
            {
                FingerSuccess();
            }
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
