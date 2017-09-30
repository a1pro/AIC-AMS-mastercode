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
    public class DashboardViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        Department dept = new Department();

        public event Action UpdateOxyplot;

        private readonly User _user;

        PassingValue pv = new PassingValue();

        private readonly DashboardFactory _factory;

        private static DateTime _now = DateTime.Now;

        private static readonly HijriDate _nowHijri = DateLocaleConvert.ConvertGregorianToHijri(DateTime.Now);

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public ICommand ChangeLangCommand { get; }

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();

        public ObservableCollection<string> SummaryTypes { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<KeyValuePair<int, string>> Months { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public ObservableCollection<int> Years { get; set; } = new ObservableCollection<int>();

        public ObservableCollection<int> Days { get; set; } = new ObservableCollection<int>();

        private CommonSummary _summary;

        public CommonSummary Summary
        {
            get { return _summary; }
            set { SetProperty(ref _summary, value); }
        }

        private PlotModel _oxyModel;

        public PlotModel OxyModel
        {
            get { return _oxyModel; }
            set { SetProperty(ref _oxyModel, value); }
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

        private bool _selectedDaySummaryType = false;

        public bool IsDayVisible => _selectedDaySummaryType;

        public bool IsMonthVisible => !_selectedDaySummaryType;

        public string SelectedSummaryType
        {
            get { return _selectedDaySummaryType ? Resource.DaySummaryText : Resource.MonthSummaryText; }
            set
            {
                SetProperty(ref _selectedDaySummaryType, value == Resource.DaySummaryText);
                OnPropertyChanged(nameof(IsDayVisible));
                OnPropertyChanged(nameof(IsMonthVisible));
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

        private int _selectedDay = _now.Day;

        public int SelectedDay
        {
            get { return _selectedDay; }
            set
            {
                SetProperty(ref _selectedDay, value);
            }
        }

        private int _selectedYear;

        public int SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                SetProperty(ref _selectedYear, value);
                FillDays();
            }
        }

        private KeyValuePair<int, string> _selectedMonth;

        public KeyValuePair<int, string> SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                SetProperty(ref _selectedMonth, value);
                FillDays();
            }
        }

        private bool _isChartsVisible;

        public bool IsChartsVisible
        {
            get { return _isChartsVisible; }
            set { SetProperty(ref _isChartsVisible, value); }
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

        public DashboardViewModel()
        {
            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user == null)
            {
                NavigationService.SetPage(new LoginViewModel());
                //   DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
                CrossLocalNotifications.Current.Show("AicAms", "UnAuthed", pv.notificationid, DateTime.Now);
                pv.notificationid++;
                return;
            }
            _factory = new DashboardFactory();
            RefreshCommand = new Command(Refresh);
            ShowCommand = new Command(Show);
            

            SummaryTypes = new ObservableCollection<string>
            {
                Resource.MonthSummaryText,
                Resource.DaySummaryText,
            };

            FillCalendar();

            if (IsManager)
            {
                FillDepartments();
            }


            ChangeLangCommand = new Command(LocaleHelper.ChangeCulture);
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
                CultureInfo _ci = Settings.Culture;
              

                foreach (var department in res.Data)
                {
                    if (_ci.Name == "en-US")
                    {
                        string depttest = department.Name.ToString().Trim().Replace(" ", "_");
                        string translation = Resource.ResourceManager.GetString(depttest, _ci) ?? depttest;
                        dept.Id = department.Id;
                        dept.Name = translation;
                        Departments.Add(dept);
                    }
                    else
                    {
                        Departments.Add(department);
                    }
                  
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
                await ShowCharts();
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
            ShowCharts();
        }

        private async Task ShowCharts()
        {
            if (SelectedEmployee == null && IsManager)
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }
            
            OperationResult<CommonSummary> res;
            CancellAll();
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;
            if (_selectedDaySummaryType)
            {
                var ticks = _user.IsGregorianLocale ? new DateTime(SelectedYear, SelectedMonth.Key, SelectedDay).Ticks : DateLocaleConvert.ConvertHijriToGregorian(new HijriDate
                {
                    Year = SelectedYear,
                    Month = SelectedMonth.Key,
                    Day = SelectedDay
                }).Ticks;
                if (!IsManager)
                    res = await _factory.DaySummary(_user.Token, ticks, _cancellationToken.Token);
                else
                    res = await _factory.DaySummaryForEmployee(_user.Token, SelectedEmployee.Id, ticks,
                        _cancellationToken.Token);
            }
            else
            {
                var d = _user.IsGregorianLocale
                    ? new DateTime(SelectedYear, SelectedMonth.Key, 1)
                    : DateLocaleConvert.ConvertHijriToGregorian(new HijriDate
                    {
                        Year = SelectedYear,
                        Month = SelectedMonth.Key,
                        Day = 1
                    });
                long startTicks = d.Ticks;
                long endTicks = _user.IsGregorianLocale
                    ? d.AddMonths(1).Ticks
                    : DateLocaleConvert.ConvertHijriToGregorian(new HijriDate
                    {
                        Year = SelectedMonth.Key == 12 ? SelectedYear + 1 : SelectedYear,
                        Month = SelectedMonth.Key == 12 ? 1 : SelectedMonth.Key + 1,
                        Day = 1
                    }).Ticks;

                if (!IsManager)
                    res = await _factory.MonthSummary(_user.Token, startTicks, endTicks, _cancellationToken.Token);
                else
                    res = await _factory.MonthSummaryForEmployee(_user.Token, SelectedEmployee.Id, startTicks, endTicks, _cancellationToken.Token);
            }
            IsIndicatorVisible = false;
            if (res.ResultCode == ResultCode.Success)
            {
                if (res.Data == null)
                {
                    IsNoDataMsgVisible = true;
                    IsChartsVisible = false;
                }
                else
                {
                    res.Data.Calculate();
                    res.Data.Status = res.Data.Status?.Trim();
                    Summary = res.Data;
                    switch (Summary.Status)
                    {
                        case "P":
                            Status = Resource.StatusP;
                            break;

                        case "A":
                            Status = Resource.StatusA;
                            break;

                        case "WE":
                            Status = Resource.StatusWE;
                            break;

                        case "H":
                            Status = Resource.StatusH;
                            break;

                        case "PE":
                            Status = Resource.StatusPE;
                            break;

                        case "UE":
                            Status = Resource.StatusUE;
                            break;

                        case "CM":
                            Status = Resource.StatusCM;
                            break;

                        case "JB":
                            Status = Resource.StatusJB;
                            break;

                        default:
                            Status = Resource.StatusUnknown;
                            break;
                    }

                    OxyModel = CreatePlotModel(Summary.GapDurationWithoutExcuse, Summary.WorkDuration, Summary.ShiftDuration);
                    IsChartsVisible = true;
                    CancellAll();
                    FillChartsAnimation(res.Data.TotalWorkingHoursPercent, res.Data.TotalAbsentDaysPercent, res.Data.TotalLateHoursPercent, _cancellationToken.Token);

                    UpdateOxyplot?.Invoke();
                }
            }
        }

        private PlotModel CreatePlotModel(double first, double second, double third)
        {
            first /= 3600;
            second /= 3600;
            third /= 3600;
            var max = Math.Max(first, Math.Max(second, third));

            max = Math.Abs(max) < 0.1 ? 1 : max * 1.1;

            return new PlotModel
            {
                Axes =
                {
                    new CategoryAxis
                    {
                        Position = AxisPosition.Bottom,
                        AbsoluteMinimum = -0.5,
                        AbsoluteMaximum = 2.5,
                        TextColor = OxyColors.Transparent,

                    },
                    new LinearAxis
                    {
                        Title = Resource.HoursText,
                        Position = AxisPosition.Left,
                        AbsoluteMinimum = 0,
                        AbsoluteMaximum = max
                    }
                },
                Series =
                {
                    new ColumnSeries
                    {
                        Items =
                        {
                            new ColumnItem
                            {
                                Value = first,
                                Color = OxyColors.Red
                            },
                            new ColumnItem
                            {
                                Value = second,
                                Color = OxyColors.Green
                            },
                            new ColumnItem
                            {
                                Value = third,
                                Color = OxyColors.Blue
                            },
                            new ColumnItem
                            {
                                Value = max,
                                Color = OxyColors.Transparent
                            }
                        }
                    }
                }
            };
        }

        private void FillChartsAnimation(double totalWorkingHoursPercent, double totalAbsentDaysPercent, double totalLateHoursPercent, CancellationToken token)
        {
            var t = 100;

            Summary.TotalAbsentDaysPercent = 0;
            Summary.TotalLateHoursPercent = 0;
            Summary.TotalWorkingHoursPercent = 0;
            OnPropertyChanged(nameof(Summary));

            var twhp = totalWorkingHoursPercent / t;
            var tadp = totalAbsentDaysPercent / t;
            var tlhp = totalLateHoursPercent / t;

            var c = 1;
            Device.StartTimer(TimeSpan.FromMilliseconds(10), delegate
            {
                if (token.IsCancellationRequested)
                    return false;
                if (c == t)
                {
                    Summary.TotalAbsentDaysPercent = totalAbsentDaysPercent;
                    Summary.TotalLateHoursPercent = totalLateHoursPercent;
                    Summary.TotalWorkingHoursPercent = totalWorkingHoursPercent;
                    OnPropertyChanged(nameof(Summary));
                    return false;
                }

                c++;
                Summary.TotalAbsentDaysPercent += tadp;
                Summary.TotalLateHoursPercent += tlhp;
                Summary.TotalWorkingHoursPercent += twhp; 
                OnPropertyChanged(nameof(Summary));
                return true;
            });
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

        private void FillDays()
        {
            if (SelectedMonth.Key == 0 || SelectedYear == 0)
                return;

            var d = _user.IsGregorianLocale ? DateTime.DaysInMonth(SelectedYear, SelectedMonth.Key) : DateLocaleConvert.HijriDaysInMonth(SelectedYear, SelectedMonth.Key);
            if (Days.Count != d)
            {
                Days.Clear();
                Days.AddRange(Enumerable.Range(1, d));
            }
            SelectedDay = 1;
        }
    }
}
