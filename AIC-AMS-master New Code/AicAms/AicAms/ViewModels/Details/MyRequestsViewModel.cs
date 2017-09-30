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
using AicAms.Models.Local;
using AicAms.Models.Reports;
using AicAms.Models.Summary;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels.Details
{
    public class MyRequestsViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly ReportsFactory _factory;

        PassingValue pv = new PassingValue();

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public Color FirstColor => Color.FromRgb(254, 255, 252);

        public Color SecondColor => Color.FromRgb(219, 216, 220);

        public ObservableCollection<ReportRequest> Requests { get; set; } = new ObservableCollection<ReportRequest>();

        public ObservableCollection<string> RequestTypes { get; set; } = new ObservableCollection<string>
        {
            Resource.AllDdl,
            Resource.ExcuseDdl,
            Resource.VacDdl
        };

        public ObservableCollection<string> Statuses { get; set; } = new ObservableCollection<string>
        {
            Resource.AllDdl,
            Resource.UnderProcessDdl,
            Resource.AcceptedDdl,
            Resource.RejectedDdl,
        };

        public DateTime MaxDate => DateTime.Today;

        public DateTime MinTime => DateTime.Today.AddYears(-10);

        private DateTime _dateFrom = DateTime.Today.AddDays(-7);

        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set { SetProperty(ref _dateFrom, value); }
        }

        private DateTime _dateTo = DateTime.Today;

        public DateTime DateTo
        {
            get { return _dateTo; }
            set { SetProperty(ref _dateTo, value); }
        }

        private string _selectedType = Resource.AllDdl;

        public string SelectedType
        {
            get { return _selectedType; }
            set { SetProperty(ref _selectedType, value); }
        }

        private string _selectedStatus = Resource.UnderProcessDdl;

        public string SelectedStatus
        {
            get { return _selectedStatus; }
            set { SetProperty(ref _selectedStatus, value); }
        }

        private bool _isRefreshBusy;

        public bool IsRefreshBusy
        {
            get { return _isRefreshBusy; }
            set { SetProperty(ref _isRefreshBusy, value); }
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

        private bool _isNoDataMsgVisible;

        public bool IsNoDataMsgVisible
        {
            get { return _isNoDataMsgVisible; }
            set { SetProperty(ref _isNoDataMsgVisible, value); }
        }

        private bool _isGridTitleVisible;

        public bool IsGridTitleVisible
        {
            get { return _isGridTitleVisible; }
            set { SetProperty(ref _isGridTitleVisible, value); }
        }


        public MyRequestsViewModel()
        {
            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user == null)
            {
                NavigationService.SetPage(new LoginViewModel());
                // DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
                CrossLocalNotifications.Current.Show("AicAms", "UnAuthed", pv.notificationid, DateTime.Now);
                pv.notificationid++;
                return;
            }

            _factory = new ReportsFactory();

            RefreshCommand = new Command(Refresh);
            ShowCommand = new Command(Show);
        }

        private void Refresh()
        {
            IsRefreshBusy = true;

            Show();

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

        private async void Show()
        {
            if (DateFrom > DateTo)
            {
                await MessageViewer.ErrorAsync(Resource.MyRequestsValidationError);
                return;
            }

            CancellAll();
            Requests.Clear();
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;
            IsGridTitleVisible = false;

            var status = SelectedStatus == Resource.AllDdl
                ? -1
                : SelectedStatus == Resource.UnderProcessDdl
                    ? 0
                    : SelectedStatus == Resource.AcceptedDdl
                        ? 1
                        : 2;

            var ret = SelectedType == Resource.AllDdl
                ? 0
                : SelectedType == Resource.ExcuseDdl
                    ? 1
                    : 2;

            //end of day
            var to = DateTo.Add(new TimeSpan(23, 59, 59));

            var res = await _factory.GetMyRequests(_user.Token, status, ret, DateFrom.Ticks, to.Ticks, _cancellationToken.Token);

            IsIndicatorVisible = false;
            if (res.ResultCode == ResultCode.Success)
            {
                if (res.Data == null || res.Data.Length == 0)
                {
                    IsNoDataMsgVisible = true;
                }
                else
                {
                    IsGridTitleVisible = true;

                    var i = 0;
                    foreach (var req in res.Data)
                    {
                        req.BackgroundColor = i++ % 2 == 0 ? FirstColor : SecondColor;
                        req.Calculate();
                        Requests.Add(req);
                    }
                }
            }
        }

        public void CancellAll()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
