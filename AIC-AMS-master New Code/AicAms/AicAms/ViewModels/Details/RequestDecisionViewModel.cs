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
using AicAms.Models.Reports;
using AicAms.Models.Summary;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using AicAms.ViewModels.Popups;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels.Details
{
    public class RequestDecisionViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly ReportsFactory _repFactory;

        PassingValue pv = new PassingValue();

        private readonly DashboardFactory _boardFactory;

        private static DateTime _now = DateTime.Now;

        private static readonly HijriDate _nowHijri = DateLocaleConvert.ConvertGregorianToHijri(DateTime.Now);

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public ICommand CheckCommand { get; }

        public ICommand ApproveCommand { get; }

        public ICommand RejectCommand { get; }

        public ICommand ViewInfoCommand { get; }

        public Color FirstColor => Color.FromRgb(254, 255, 252);

        public Color SecondColor => Color.FromRgb(219, 216, 220);

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();

        public ObservableCollection<ReportRequest> Requests { get; set; } = new ObservableCollection<ReportRequest>();

        public ObservableCollection<KeyValuePair<int, string>> Months { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public ObservableCollection<int> Years { get; set; } = new ObservableCollection<int>();

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

        private bool _isAllEmps;

        public bool IsAllEmps
        {
            get { return _isAllEmps; }
            set
            {
                SetProperty(ref _isAllEmps, value);
                OnPropertyChanged(nameof(IsDepAndEmpVisible));
                if (!value && Departments.Count == 0)
                {
                    FillDepartments();
                }
            }
        }

        public bool IsDepAndEmpVisible => !IsAllEmps;

        private bool _isBlockingDisplay;

        public bool IsBlockingDisplay
        {
            get { return _isBlockingDisplay; }
            set { SetProperty(ref _isBlockingDisplay, value); }
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

        public bool IsFooterVisible => Requests.Count > 0;

        public RequestDecisionViewModel()
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

            _repFactory = new ReportsFactory();
            _boardFactory = new DashboardFactory();

            RefreshCommand = new Command(Refresh);
            ShowCommand = new Command(Show);
            CheckCommand = new Command<ReportRequest>(Check);
            ApproveCommand = new Command(Approve);
            RejectCommand = new Command(Reject);
            ViewInfoCommand = new Command<ReportRequest>(ViewInfo);

            IsAllEmps = true;
            FillCalendar();
        }

        private void FillCalendar()
        {
            var year = _user.IsGregorianLocale ? _now.Year : _nowHijri.Year;
            var end = year - 10;
            for (var i = year; i >= end; i--)
                Years.Add(i);

            var cul = _user.IsGregorianLocale ? new CultureInfo("en-US") : new CultureInfo("ar-SA");
            for (var i = 1; i <= 12; i++)
                Months.Add(new KeyValuePair<int, string>(i, cul.DateTimeFormat.GetMonthName(i)));

            SelectedYear = year;
            SelectedMonth = Months.FirstOrDefault(m => m.Key == _now.Month);
        }

        private async void ViewInfo(ReportRequest report)
        {
            await NavigationService.NavigatePopup(new RequestInfoViewModel(report));
        }

        private void Approve()
        {
            ApproveOrReject(true);
        }

        private void Reject()
        {
            ApproveOrReject(false);
        }

        private async void ApproveOrReject(bool isApprove)
        {
            IsBlockingDisplay = true;
            var col = Requests.Where(e => e.IsSelected).ToArray();
            if (col.Any())
            {
                var isVacTypes = new StringBuilder();
                var erqIds = new StringBuilder();
                foreach (var request in col)
                {
                    erqIds.Append(request.Id + ",");
                    isVacTypes.Append((request.RetId == "VAC") + ",");
                }
                var res = await _repFactory.RequestDecision(_user.Token, isVacTypes.ToString().TrimEnd(','), erqIds.ToString().TrimEnd(','), _user.Login, isApprove, _cancellationToken.Token);
                if (res.ResultCode == ResultCode.Success)
                {
                    if (res.Data)
                    {
                        MessageViewer.SuccessAsync(Resource.SuccessSentRequest);
                        foreach (var request in col)
                        {
                            Requests.Remove(request);
                        }
                        if (Requests.Count > 0)
                        {
                            var i = 0;
                            foreach (var request in Requests)
                            {
                                request.BackgroundColor = i++ % 2 == 0 ? FirstColor : SecondColor;
                            }
                        }
                        else
                        {
                            OnPropertyChanged(nameof(IsFooterVisible));
                            IsNoDataMsgVisible = true;
                            IsGridTitleVisible = false;
                        }
                    }
                    else
                    {
                        MessageViewer.ErrorAsync(Resource.FailureSentRequst);
                    }
                }
            }
            else
            {
                MessageViewer.ErrorAsync(Resource.NotSelectedError);
            }
            IsBlockingDisplay = false;
        }

        private void Check(ReportRequest req)
        {
            req.IsSelected = !req.IsSelected;
        }

        private async void Refresh()
        {
            CancellAll();
            IsRefreshBusy = true;
            if (IsAllEmps)
            {
                await ShowGrid();
            }
            else if (Departments.Count == 0)
            {
                await FillDepartments();
            }
            else if (SelectedDepartment != null && Employees.Count == 0)
            {
                await FillEmployees();
            }
            else if (SelectedEmployee != null)
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

        private async Task ShowGrid()
        {
            if (!IsAllEmps && SelectedEmployee == null)
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }

            CancellAll();
            Requests.Clear();
            OnPropertyChanged(nameof(IsFooterVisible));
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;
            IsGridTitleVisible = false;

            DateTime from;
            DateTime to;
            if (_user.IsGregorianLocale)
            {
                from = new DateTime(SelectedYear, SelectedMonth.Key, 1);
                to = from.AddMonths(1).AddSeconds(-1);
            }
            else
            {
                from = DateLocaleConvert.ConvertHijriToGregorian(new HijriDate
                {
                    Day = 1,
                    Month = SelectedMonth.Key,
                    Year = SelectedYear
                });
                to = from.AddDays(DateLocaleConvert.HijriDaysInMonth(SelectedYear, SelectedMonth.Key)).AddSeconds(-1);
            }

            var res = await _repFactory.RequestsForEmp(_user.Token, IsAllEmps ? string.Empty : SelectedEmployee.Id, from.Ticks, to.Ticks, _cancellationToken.Token);

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
                    foreach (var q in res.Data)
                    {
                        q.Calculate(_user.IsGregorianLocale);
                        q.BackgroundColor = i++ % 2 == 0 ? FirstColor : SecondColor;
                        q.EmpLogin = q.EmpLogin.Trim();
                        Requests.Add(q);
                    }
                    OnPropertyChanged(nameof(IsFooterVisible));
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
            var res = await _boardFactory.GetEmployeesForDepartment(_user.Token, SelectedDepartment.Id, _cancellationToken.Token);
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

        public void CancellAll()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
