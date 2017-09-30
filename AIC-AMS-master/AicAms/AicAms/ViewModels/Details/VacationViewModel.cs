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

namespace AicAms.ViewModels.Details
{
    public class VacationViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly ReportsFactory _factory;

        public ICommand RefreshCommand { get; set; }

        public ICommand SendCommand { get; set; }

        public ObservableCollection<RequestType> RequestTypes { get; set; } = new ObservableCollection<RequestType>();

        private RequestType _selectedRequestType = null;

        public RequestType SelectedRequestType
        {
            get { return _selectedRequestType; }
            set { SetProperty(ref _selectedRequestType, value); }
        }

        public DateTime MaxDate => DateTime.Today.AddYears(1);

        public DateTime MinTime => DateTime.Today.AddMonths(-6);

        private DateTime _dateFrom = DateTime.Today;

        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set { SetProperty(ref _dateFrom, value); }
        }

        private DateTime _dateTo = DateTime.Today.AddDays(1);

        public DateTime DateTo
        {
            get { return _dateTo; }
            set { SetProperty(ref _dateTo, value); }
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

        private string _vacTypesPikerTitle = Resource.VacationTypeTitle;

        public string VacationTypesPikerTitle
        {
            get { return _vacTypesPikerTitle; }
            set { SetProperty(ref _vacTypesPikerTitle, value); }
        }

        private string _reason;

        public string Reason
        {
            get { return _reason; }
            set { SetProperty(ref _reason, value); }
        }

        public VacationViewModel()
        {
            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user == null)
            {
                NavigationService.SetPage(new LoginViewModel());
                DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
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
            VacationTypesPikerTitle = Resource.DownloadingText;
            var res = await _factory.GetVacationTypes(_cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                SelectedRequestType = null;
                RequestTypes.Clear();
                foreach (var req in res.Data)
                {
                    RequestTypes.Add(req);
                }
            }
            VacationTypesPikerTitle = Resource.VacationTypeTitle;
        }

        private async void Send()
        {
            if (SelectedRequestType == null || string.IsNullOrWhiteSpace(Reason))
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }
            if (DateFrom >= DateTo)
            {
                await MessageViewer.ErrorAsync(Resource.VacationValidationError);
                return;
            }
            CancellAll();
            IsIndicatorVisible = true;
            var res = await _factory.SendVacation(_user.Token, SelectedRequestType.Id, DateFrom.Ticks, DateTo.Ticks, Reason, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                if (res.Data)
                {
                    await NavigationService.CurrentPage.DisplayAlert(Resource.SuccessText, Resource.SuccessSentRequest, Resource.OkText);
                    DateFrom = DateTime.Today;
                    DateTo = DateTime.Today.AddDays(1);
                    SelectedRequestType = null;
                    Reason = "";
                }
                else
                {
                    await NavigationService.CurrentPage.DisplayAlert(Resource.ErrorText, Resource.FailureSentRequst, Resource.OkText);
                }
            }
            IsIndicatorVisible = false;
        }
    }
}
