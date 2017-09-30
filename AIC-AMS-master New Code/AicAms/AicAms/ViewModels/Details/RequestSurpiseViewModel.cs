using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using AicAms.Models.Employee;
using AicAms.Models.Local;
using AicAms.Models.Reports;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels.Details
{
    public class RequestSurpiseViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly DashboardFactory _boardFactory;

        PassingValue pv = new PassingValue();

        private readonly SurpriseFactory _surpFactory;

      

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public ICommand CheckCommand { get; }

        public ICommand SendCommand { get; }

        public Color FirstColor => Color.FromRgb(254, 255, 252);

        public Color SecondColor => Color.FromRgb(219, 216, 220);

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();

        private Department _selectedDepartment = null;

        public Department SelectedDepartment
        {
            get { return _selectedDepartment; }
            set {  SetProperty(ref _selectedDepartment, value); }
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

        public bool IsGridTitleVisible => Employees.Count > 0;

        public bool IsFooterVisible => Employees.Count > 0;

        private string _departmentsPikerTitle = Resource.DepartmentTitle;

        public string DepartmentsPikerTitle
        {
            get { return _departmentsPikerTitle; }
            set { SetProperty(ref _departmentsPikerTitle, value); }
        }

        private bool _isBlockingDisplay;

        public bool IsBlockingDisplay
        {
            get { return _isBlockingDisplay; }
            set { SetProperty(ref _isBlockingDisplay, value); }
        }

        public RequestSurpiseViewModel()
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

            _boardFactory = new DashboardFactory();
            _surpFactory = new SurpriseFactory();

            RefreshCommand = new Command(Refresh);
            ShowCommand = new Command(Show);
            CheckCommand = new Command<Employee>(Check);
            SendCommand = new Command(Send);

            FillDepartments();

            Employees.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs args)
            {
                OnPropertyChanged(nameof(IsGridTitleVisible));
                OnPropertyChanged(nameof(IsFooterVisible));
            };
        }

        public async void Send()
        {
            IsBlockingDisplay = true;

            var emps = Employees.Where(e => e.IsSelected).Select(e => e.Id).ToArray();
            if (emps.Any())
            {
                var res = await _surpFactory.SendExcuse(_user.Token, emps, _cancellationToken.Token);
                if (res.ResultCode == ResultCode.Success)
                {
                    MessageViewer.SuccessAsync(Resource.SuccessSentRequest);
                    Employees.Clear();
                }
                else
                {
                    MessageViewer.ErrorAsync(Resource.FailureSentRequst);
                }
            }
            else
            {
                MessageViewer.ErrorAsync(Resource.NotSelectedError);
            }
            IsBlockingDisplay = false;
        }

        private async void Refresh()
        {
            CancellAll();
            IsRefreshBusy = true;

            if (Departments.Count == 0)
            {
                await FillDepartments();
            }
            else if (SelectedDepartment != null)
            {
                await ShowGrid();
            }
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

        private void Check(Employee req)
        {
            req.IsSelected = !req.IsSelected;
        }

        private void Show()
        {
            ShowGrid();
        }

        private async Task ShowGrid()
        {
            if (SelectedDepartment == null)
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }

            CancellAll();
            Employees.Clear();
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;

            var res = await _boardFactory.GetEmployeesForDepartment(_user.Token, SelectedDepartment.Id, _cancellationToken.Token);

            IsIndicatorVisible = false;
            if (res.ResultCode == ResultCode.Success)
            {
                if (res.Data == null || res.Data.Length == 0)
                {
                    IsNoDataMsgVisible = true;
                }
                else
                {
                    var i = 0;
                    foreach (var q in res.Data)
                    {
                        q.Id = q.Id.Trim();
                        q.BackgroundColor = i++ % 2 == 0 ? FirstColor : SecondColor;
                        Employees.Add(q);
                    }
                }
            }
        }

        private async Task FillDepartments()
        {
            CancellAll();
            DepartmentsPikerTitle = Resource.DownloadingText;
            var res = await _boardFactory.GetDepartments(_user.Token, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                SelectedDepartment = null;
                Departments.Clear();
                foreach (var department in res.Data)
                {
                    Departments.Add(department);
                }
            }
            DepartmentsPikerTitle = Resource.DepartmentTitle;
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
