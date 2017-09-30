using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using AicAms.Models.Summary;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels.Details
{
    public class MonthSummaryViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        PassingValue pv = new PassingValue();

        private readonly DashboardFactory _factory;

        private static DateTime _now = DateTime.Now;

        private static readonly HijriDate _nowHijri = DateLocaleConvert.ConvertGregorianToHijri(DateTime.Now);

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public ObservableCollection<AttendanceList> Summaries { get; set; } = new ObservableCollection<AttendanceList>();

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();

        public ObservableCollection<KeyValuePair<int, string>> Months { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public ObservableCollection<int> Years { get; set; } = new ObservableCollection<int>();

        public ObservableCollection<int> Days { get; set; } = new ObservableCollection<int>();

        public Color FirstColor => Color.FromRgb(254, 255, 252);

        public Color SecondColor => Color.FromRgb(219, 216, 220);

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

        private int _selectedYear;

        public int SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                SetProperty(ref _selectedYear, value);
            }
        }

        private KeyValuePair<int, string> _selectedMonth;

        public KeyValuePair<int, string> SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                SetProperty(ref _selectedMonth, value);
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

        private bool _isGridTitleVisible;

        public bool IsGridTitleVisible
        {
            get { return _isGridTitleVisible; }
            set { SetProperty(ref _isGridTitleVisible, value); }
        }

        public MonthSummaryViewModel()
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

            FillCalendar();

            if (IsManager)
            {
                FillDepartments();
            }
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

        private void Show()
        {
            ShowGrid();
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

        private void FillCalendar()
        {
            var year = _user.IsGregorianLocale ? _now.Year : _nowHijri.Year;
            for (var i = year; i >= year - 10; i--)
                Years.Add(i);

            var cul = _user.IsGregorianLocale ? new CultureInfo("en-US") : new CultureInfo("ar-SA");
            for (var i = 1; i <= 12; i++)
                Months.Add(new KeyValuePair<int, string>(i, cul.DateTimeFormat.GetMonthName(i)));

            SelectedYear = year;
            SelectedMonth = Months.FirstOrDefault(m => m.Key == _now.Month);
        }

        private async Task ShowGrid()
        {
            if (SelectedEmployee == null && IsManager)
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }

            OperationResult<AttendanceList[]> res;
            CancellAll();
            Summaries.Clear();
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;
            IsGridTitleVisible = false;

            var d = _user.IsGregorianLocale
                ? new DateTime(SelectedYear, SelectedMonth.Key, 1)
                : DateLocaleConvert.ConvertHijriToGregorian(new HijriDate
                {
                    Year = SelectedYear,
                    Month = SelectedMonth.Key,
                    Day = 1
                });
            var startTicks = d.Ticks;
            var endTicks = _user.IsGregorianLocale
                ? d.AddMonths(1).AddSeconds(-1).Ticks
                : d.AddDays(DateLocaleConvert.HijriDaysInMonth(SelectedYear, SelectedMonth.Key)).AddSeconds(-1).Ticks;

            if (!IsManager)
                res = await _factory.AttendanceListViews(_user.Token, startTicks, endTicks, _cancellationToken.Token);
            else
                res = await _factory.AttendanceListViewsForEmp(_user.Token, SelectedEmployee.Id, startTicks, endTicks, _cancellationToken.Token);

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
                    foreach (var shiftSummary in res.Data)
                    {
                        shiftSummary.Status = shiftSummary.Status?.Trim();
                        shiftSummary.Status = shiftSummary.Status == "P" ? "OW" : shiftSummary.Status;
                        shiftSummary.Calculate(_user.IsGregorianLocale, true);
                        shiftSummary.BackgroundColor = i++ % 2 == 0 ? FirstColor : SecondColor;
                        Summaries.Add(shiftSummary);
                    }
                }
            }
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
 