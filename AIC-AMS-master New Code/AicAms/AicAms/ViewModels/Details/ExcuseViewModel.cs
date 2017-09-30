using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AicAms.ApiFactory;
using AicAms.Helpers;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Models.Department;
using AicAms.Models.Reports;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels.Details
{
    public class ExcuseViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly ReportsFactory _factory;

        public ICommand RefreshCommand { get; set; }

        public ICommand SendCommand { get; set; }

        PassingValue pv = new PassingValue();

        public ObservableCollection<RequestType> RequestTypes { get; set; } = new ObservableCollection<RequestType>();

        private RequestType _selectedRequestType = null;

        public RequestType SelectedRequestType
        {
            get { return _selectedRequestType; }
            set { SetProperty(ref _selectedRequestType, value); }
        }

        public DateTime MaxDate => DateTime.Today.AddMonths(6);

        public DateTime MinTime => DateTime.Today.AddMonths(-6);

        private DateTime _selectedDate = DateTime.Today;

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }

        private bool _isIndicatorVisible;

        public bool IsIndicatorVisible
        {
            get { return _isIndicatorVisible; }
            set
            {
                SetProperty(ref _isIndicatorVisible, value);
                OnPropertyChanged(nameof(IsButtonVisible));
            }
        }

        public bool IsButtonVisible => !_isIndicatorVisible;

        private bool _isRefreshBusy;

        public bool IsRefreshBusy
        {
            get { return _isRefreshBusy; }
            set { SetProperty(ref _isRefreshBusy, value); }
        }

        private string _excuseTypesPikerTitle = Resource.ExcuseTypeTitle;

        public string ExcuseTypesPikerTitle
        {
            get { return _excuseTypesPikerTitle; }
            set { SetProperty(ref _excuseTypesPikerTitle, value); }
        }

        private TimeSpan _timeFrom;

        public TimeSpan TimeFrom
        {
            get { return _timeFrom; }
            set { SetProperty(ref _timeFrom, value); }
        }

        private TimeSpan _timeTo;

        public TimeSpan TimeTo
        {
            get { return _timeTo; }
            set { SetProperty(ref _timeTo, value); }
        }

        private string _reason;

        public string Reason
        {
            get { return _reason; }
            set { SetProperty(ref _reason, value); }
        }

        public ExcuseViewModel()
        {
            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user == null)
            {
                NavigationService.SetPage(new LoginViewModel());
                //DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
                CrossLocalNotifications.Current.Show("AicAms", "UnAuthed", pv.notificationid, DateTime.Now);
                pv.notificationid++;
                return;
            }
            _factory = new ReportsFactory();

            RefreshCommand = new Command(Refresh);
            SendCommand = new Command(Send);

            FillReqTypes();
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        private async void Refresh()
        {
            CancellAll();
            IsRefreshBusy = true;
            if (RequestTypes.Count == 0)
                await FillReqTypes();
            StopRefresh();
        }

        private void StopRefresh()
        {
            Task.Run(async delegate
            {
                await Task.Delay(300);
                IsRefreshBusy = false;
            });
        }

        private async Task FillReqTypes()
        {
            CancellAll();
            ExcuseTypesPikerTitle = Resource.DownloadingText;
            var res = await _factory.GetExcuseTypes(_cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                SelectedRequestType = null;
                RequestTypes.Clear();
                foreach (var req in res.Data)
                {
                    RequestTypes.Add(req);
                }
            }
            ExcuseTypesPikerTitle = Resource.ExcuseTypeTitle;
        }

        private async void Send()
        {
            if (SelectedRequestType == null || string.IsNullOrWhiteSpace(Reason))
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }
            if (TimeFrom >= TimeTo)
            {
                await MessageViewer.ErrorAsync(Resource.ExcuseValidationError);
                return;
            }

            CancellAll();
            IsIndicatorVisible = true;
            var res = await _factory.SendExcuse(_user.Token, SelectedRequestType.Id, SelectedDate.Ticks, SelectedDate.Add(TimeFrom).Ticks, SelectedDate.Add(TimeTo).Ticks, Reason, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                if (res.Data)
                {
                    await MessageViewer.SuccessAsync(Resource.SuccessSentRequest);
                    SelectedDate = DateTime.Today;
                    TimeFrom = new TimeSpan();
                    TimeTo = new TimeSpan();
                    SelectedRequestType = null;
                    Reason = "";
                }
                else
                {
                    await MessageViewer.ErrorAsync(Resource.FailureSentRequst);
                }
            }
            IsIndicatorVisible = false;
           
        }
    }
}
