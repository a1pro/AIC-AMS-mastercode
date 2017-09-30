using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AicAms.ApiFactory;
using AicAms.DependencyServices;
using AicAms.Extensions;
using AicAms.Helpers;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Models.Department;
using AicAms.Models.Employee;
using AicAms.Models.Local;
using AicAms.Models.Summary;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using AicAms.Views.Details;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels.Details
{
    public class DaySummaryViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly DashboardFactory _factory;

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();

        public ObservableCollection<byte> Shifts { get; set; } = new ObservableCollection<byte>
        {
            1, 2, 3
        };

        PassingValue pv = new PassingValue();

        public DateTime MaxDate => DateTime.Today;

        public DateTime MinTime => DateTime.Today.AddYears(-10);

        private DateTime _date = DateTime.Today;

        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value); }
        }

        private ShiftSummary _summary;

        public ShiftSummary Summary
        {
            get { return _summary; }
            set { SetProperty(ref _summary, value); }

        }

        private bool _isTextChartsVisible;

        public bool IsTextChartsVisible
        {
            get { return _isTextChartsVisible; }
            set { SetProperty(ref _isTextChartsVisible, value); }
        }

        private Department _selectedDepartment = null;

        public Department SelectedDepartment
        {
            get { return _selectedDepartment; }
            set
            {
                SetProperty(ref _selectedDepartment, value);
                if (value != null)
                    FillEmployees();
            }
        }

        private Employee _selectedEmployee = null;

        public Employee SelectedEmployee
        {
            get { return _selectedEmployee; }
            set
            {
                SetProperty(ref _selectedEmployee, value);
            }
        }

        private string _departmentsPikerTitle = Resource.DepartmentTitle;

        public string DepartmentsPikerTitle
        {
            get { return _departmentsPikerTitle; }
            set { SetProperty(ref _departmentsPikerTitle, value); }
        }

        private string _employeePikerTitle = Resource.EmployeeTitle;

        public string EmployeePikerTitle
        {
            get { return _employeePikerTitle; }
            set { SetProperty(ref _employeePikerTitle, value); }
        }

        private bool _isRefreshBusy;

        public bool IsRefreshBusy
        {
            get { return _isRefreshBusy; }
            set { SetProperty(ref _isRefreshBusy, value); }
        }

        public bool IsManager => _user.IsManager;

        private byte _selectedShift;

        public byte SelectedShift
        {
            get { return _selectedShift; }
            set
            {
                SetProperty(ref _selectedShift, value);
            }
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

        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public DaySummaryViewModel()
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
            _factory = new DashboardFactory();
            RefreshCommand = new Command(Refresh);
            ShowCommand = new Command(Show);

            SelectedShift = 1;

            if (IsManager)
            {
                FillDepartments();
            }
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        private async Task FillDepartments()
        {
            CancellAll();
            DepartmentsPikerTitle = Resource.DownloadingText;
            var res = await _factory.GetDepartments(_user.Token, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                SelectedDepartment = null;
                SelectedEmployee = null;
                Departments.Clear();
                Employees.Clear();
                foreach (var department in res.Data)
                {
                    Departments.Add(department);
                }
            }
            DepartmentsPikerTitle = Resource.DepartmentTitle;
        }

        private async Task FillEmployees()
        {
            CancellAll();
            EmployeePikerTitle = Resource.DownloadingText;
            var res = await _factory.GetEmployeesForDepartment(_user.Token, SelectedDepartment.Id, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                SelectedEmployee = null;
                Employees.Clear();
                foreach (var emp in res.Data)
                {
                    Employees.Add(emp);
                }
            }
            EmployeePikerTitle = Resource.EmployeeTitle;
        }

        private async void Refresh()
        {
            CancellAll();
            IsRefreshBusy = true;
            if (IsManager && Departments.Count == 0)
            {
                await FillDepartments();
            }
            else if (IsManager && SelectedDepartment != null && Employees.Count == 0)
            {
                await FillEmployees();
            }
            else if (IsManager && SelectedEmployee != null || !IsManager)
            {
                await ShowTextCharts();
            }

            StopRefresh();
        }

        private void Show()
        {
            ShowTextCharts();
        }

        private async Task ShowTextCharts()
        {
            if (SelectedEmployee == null && IsManager)
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }

            OperationResult<ShiftSummary> res;
            CancellAll();
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;
                
            var ticks = _user.IsGregorianLocale ? new DateTime(Date.Year, Date.Month, Date.Day).Ticks : DateLocaleConvert.ConvertHijriToGregorian(new HijriDate
            {
                Year = Date.Year,
                Month = Date.Month,
                Day = Date.Day
            }).Ticks;
            if (!IsManager)
                res = await _factory.ShiftSummary(_user.Token, SelectedShift, ticks, _cancellationToken.Token);
            else
                res = await _factory.ShiftSummaryForEmployee(_user.Token, SelectedEmployee.Id, SelectedShift, ticks,
                    _cancellationToken.Token);
                
            IsIndicatorVisible = false;
            if (res.ResultCode == ResultCode.Success)
            {
                if (res.Data == null)
                {
                    IsNoDataMsgVisible = true;
                    IsTextChartsVisible = false;
                }
                else
                {
                    res.Data.Status = res.Data.Status?.Trim();
                    Summary = res.Data;
                    Summary.Calculate();
                    switch (Summary.Status)
                    {
                        case "P":
                            Status = Resource.StatusP;
                            break;

                        case "A":
                            Status = Resource.StatusA;
                            break;

                        case "IN":
                            Status = Resource.StatusIN;
                            break;

                        case "OU":
                            Status = Resource.StatusOU;
                            break;

                        case "OI":
                            Status = Resource.StatusOI;
                            break;

                        default:
                            Status = Resource.StatusUnknown;
                            break;
                    }
                    OnPropertyChanged(nameof(Status));
                    IsTextChartsVisible = true;
                    CancellAll();
                }
            }
        }

        private void StopRefresh()
        {
            Task.Run(async delegate
            {
                await Task.Delay(300);
                IsRefreshBusy = false;
            });
        }
    }
}
