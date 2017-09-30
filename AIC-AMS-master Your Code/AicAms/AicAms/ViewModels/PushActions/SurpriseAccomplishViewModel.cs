﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using AicAms.ViewModels.Auth;
using Plugin.Fingerprint;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace AicAms.ViewModels.PushActions
{
    public class SurpriseAccomplishViewModel : ObservableObject, ICancellable
    {
        private readonly int _requestId;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly SurpriseFactory _surpFactory;

        public ICommand CancelCommand { get; }

        public ICommand SendCommand { get; }

        private IFingerprint _fingerprint = DependencyService.Get<IFingerprint>();

        private bool _cancelledByUser;

        private bool _isBlockingDisplay;

        public bool IsBlockingDisplay
        {
            get { return _isBlockingDisplay; }
            set { SetProperty(ref _isBlockingDisplay, value); }
        }

        private string _blockingText;

        public string BlockingText
        {
            get { return _blockingText; }
            set { SetProperty(ref _blockingText, value); }
        }

        public SurpriseAccomplishViewModel(int requestId)
        {
            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user == null)
            {
                NavigationService.SetPage(new LoginViewModel());
                DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
                return;
            }
            _surpFactory = new SurpriseFactory();

            _requestId = requestId;
            CancelCommand = new Command(Cancel);
            SendCommand = new Command(Send);

            var wis = CrossFingerprint.Current.IsAvailableAsync();
            wis.Wait();
            if (Device.RuntimePlatform == Device.Android && wis.Result)
            {
                _fingerprint.StartListen(FingerSuccess);
            }
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
            if (Device.RuntimePlatform == Device.Android)
            _fingerprint.StopListen();
        }

        private async void Send()
        {
            var wis = await CrossFingerprint.Current.IsAvailableAsync();
            if (wis && Device.RuntimePlatform == Device.Android)
                return;

            CancellAll();

            if (!DependencyService.Get<IAskCapabilites>().AskGps())
            {
                IsBlockingDisplay = false;
                return;
            }

            if (wis && Device.RuntimePlatform != Device.Android)
                if (
                    !(await CrossFingerprint.Current.AuthenticateAsync(Resource.FingerprintComponentMsg))
                        .Authenticated)
                {
                    IsBlockingDisplay = false;
                    return;
                }

            FingerSuccess();
        }

        public async void FingerSuccess()
        {
            Position position = null;
            IsBlockingDisplay = true;
            BlockingText = Resource.CheckingLocationLabel;
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = Config.Configuration.DesiredAccuracy;
                position = await locator.GetPositionAsync(60000, _cancellationToken.Token);
            }
            catch (Exception)
            {
                if (!_cancelledByUser)
                    MessageViewer.Error(Resource.UnableLocationMessage);
                IsBlockingDisplay = false;
            }

            if (position == null)
            {
                IsBlockingDisplay = false;
                return;
            }

            BlockingText = Resource.SendingRequestLabel;
            var res = await _surpFactory.SendAttendace(_user.Token, _requestId, position.Latitude, position.Longitude, _cancellationToken.Token);

            if (res.ResultCode == ResultCode.Success)
            {
                await MessageViewer.SuccessAsync(Resource.SuccessSentRequest);
                await NavigationService.BackModal();
            }
            else if (res.ResultCode == ResultCode.UnknownError)
            {
                await MessageViewer.ErrorAsync(Resource.FailureSentRequst);
                if (Device.RuntimePlatform == Device.Android && await CrossFingerprint.Current.IsAvailableAsync())
                {
                    _fingerprint.StartListen(FingerSuccess);
                }
            }

            IsBlockingDisplay = false;
        }

        private void Cancel()
        {
            _cancelledByUser = true;
            CancellAll();
            NavigationService.BackModal();
        }
    }
}
