using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AicAms.Helpers;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.Views;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using AicAms.ViewModels.Details;
using AicAms.Views.Details;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace AicAms.ViewModels
{
    public class MenuViewModel : ObservableObject
    {
        private User _user;

        public ICommand SelectMenuItemCommand { get; }

        private string _welcome;

        public string WelcomeText
        {
            get { return _welcome; }
            set { SetProperty(ref _welcome, value); }
        }

        PassingValue pv = new PassingValue();

        private readonly Color _selectedColor = Color.Black;

        private readonly Color _deactiveColor = Color.FromRgba(119, 119, 119, 255);

        private Color _dashboardColor;

        public Color DashboardColor
        {
            get { return _dashboardColor; }
            set { SetProperty(ref _dashboardColor, value); }
        }

        private Color _daySummaryColor;

        public Color DaySummaryColor
        {
            get { return _daySummaryColor; }
            set { SetProperty(ref _daySummaryColor, value); }
        }

        private Color _monthSummaryColor;

        public Color MonthSummaryColor
        {
            get { return _monthSummaryColor; }
            set { SetProperty(ref _monthSummaryColor, value); }
        }

        private Color _logoutColor;

        public Color LogoutColor
        {
            get { return _logoutColor; }
            set { SetProperty(ref _logoutColor, value); }
        }

        private Color _excuseColor;

        public Color ExcuseColor
        {
            get { return _excuseColor; }
            set { SetProperty(ref _excuseColor, value); }
        }

        private Color _vacationColor;

        public Color VacationColor
        {
            get { return _vacationColor; }
            set { SetProperty(ref _vacationColor, value); }
        }

        private Color _reqColor;

        public Color RequestColor
        {
            get { return _reqColor; }
            set { SetProperty(ref _reqColor, value); }
        }

        private Color _decColor;

        public Color DecisionColor
        {
            get { return _decColor; }
            set { SetProperty(ref _decColor, value); }
        }

        private Color _reqSurColor;

        public Color ReqSurpriseColor
        {
            get { return _reqSurColor; }
            set { SetProperty(ref _reqSurColor, value); }
        }

        private Color _surpriseResColor;

        public Color SurpriseResultColor
        {
            get { return _surpriseResColor; }
            set { SetProperty(ref _surpriseResColor, value); }
        }

        private bool _isMgr;

        public bool IsManager
        {
            get { return _isMgr; }
            set
            {
                SetProperty(ref _isMgr, value);
                OnPropertyChanged(nameof(IsEmployee));
            }
        }

        public bool IsEmployee => !IsManager;

        public MenuViewModel()
        {
            SelectMenuItemCommand = new Command(SelectMenuItem);
            MessagingCenter.Subscribe<LoginViewModel>(this, "Authed", (sender) =>
            {
                _user = App.Realm.All<User>().FirstOrDefault();
                IsManager = _user.IsManager;
                WelcomeText = Resources.Resource.WelcomeText + _user.FullName.Trim();
            });
        }

        private async void SelectMenuItem(object parameter)
        {
            int pos;
            App.MS.IsPresented = false;
            if (int.TryParse(parameter as string, out pos) && pos != 0)
            {
                var opt = (SelectedMenuOptions)pos;
                (NavigationService.CurrentPage as ICancellable)?.CancellAll();
                switch (opt)
                {
                    case SelectedMenuOptions.Dashboard:
                        if (!(NavigationService.CurrentPage is DashboardPage))
                            NavigationService.SetDetailPage(new DashboardViewModel(), opt);
                        break;
                    case SelectedMenuOptions.DaySummary:
                        if (!(NavigationService.CurrentPage is DaySummaryPage))
                            NavigationService.SetDetailPage(new DaySummaryViewModel(), opt);
                        break;
                    case SelectedMenuOptions.MonthSummary:
                        if (!(NavigationService.CurrentPage is MonthSummaryPage))
                            NavigationService.SetDetailPage(new MonthSummaryViewModel(), opt);
                        break;
                    case SelectedMenuOptions.Excuse:
                        if (!(NavigationService.CurrentPage is ExcusePage))
                            NavigationService.SetDetailPage(new ExcuseViewModel(), opt);
                        break;
                    case SelectedMenuOptions.Vacation:
                        if (!(NavigationService.CurrentPage is VacationPage))
                            NavigationService.SetDetailPage(new VacationViewModel(), opt);
                        break;
                    case SelectedMenuOptions.MyRequest:
                        if (!(NavigationService.CurrentPage is MyRequestsPage))
                            NavigationService.SetDetailPage(new MyRequestsViewModel(), opt);
                        break;
                    case SelectedMenuOptions.RequestDecision:
                        if (!(NavigationService.CurrentPage is RequestDecisionPage))
                            NavigationService.SetDetailPage(new RequestDecisionViewModel(), opt);
                        break;
                    case SelectedMenuOptions.RequestSurprise:
                        if (!(NavigationService.CurrentPage is RequestSurpisePage))
                            NavigationService.SetDetailPage(new RequestSurpiseViewModel(), opt);
                        break;
                    case SelectedMenuOptions.SurpriseResult:
                        if (!(NavigationService.CurrentPage is SurpriseResultPage))
                            NavigationService.SetDetailPage(new SurpriseResultViewModel(), opt);
                        break;
                    case SelectedMenuOptions.Logout:
                        App.Realm.Write(() =>
                        {
                            App.Realm.RemoveAll<User>();
                        });
                        NavigationService.SetPage(new LoginViewModel());
                        //Device.BeginInvokeOnMainThread(delegate
                        //{
                        //    DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
                        //});

                        CrossLocalNotifications.Current.Show("AicAms", "UnAuthed", pv.notificationid, DateTime.Now);
                        pv.notificationid++;
                        break;
                }
            }
        }

        public void ConfirmMenuOptions(SelectedMenuOptions options)
        {
            DashboardColor = _deactiveColor;
            LogoutColor = _deactiveColor;
            DaySummaryColor = _deactiveColor;
            MonthSummaryColor = _deactiveColor;
            ExcuseColor = _deactiveColor;
            VacationColor = _deactiveColor;
            RequestColor = _deactiveColor;
            DecisionColor = _deactiveColor;
            ReqSurpriseColor = _deactiveColor;
            SurpriseResultColor = _deactiveColor;

            switch (options)
            {
                case SelectedMenuOptions.Dashboard:
                    DashboardColor = _selectedColor;
                    break;
                case SelectedMenuOptions.DaySummary:
                    DaySummaryColor = _selectedColor;
                    break;
                case SelectedMenuOptions.MonthSummary:
                    MonthSummaryColor = _selectedColor;
                    break;
                case SelectedMenuOptions.Excuse:
                    ExcuseColor = _selectedColor;
                    break;
                case SelectedMenuOptions.Vacation:
                    VacationColor = _selectedColor;
                    break;
                case SelectedMenuOptions.MyRequest:
                    RequestColor = _selectedColor;
                    break;
                case SelectedMenuOptions.RequestDecision:
                    DecisionColor = _selectedColor;
                    break;
                case SelectedMenuOptions.RequestSurprise:
                    ReqSurpriseColor = _selectedColor;
                    break;
                case SelectedMenuOptions.SurpriseResult:
                    SurpriseResultColor = _selectedColor;
                    break;
            }
        }
    }
}
