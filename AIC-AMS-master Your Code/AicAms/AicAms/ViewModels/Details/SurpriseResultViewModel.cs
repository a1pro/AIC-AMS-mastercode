using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AicAms.ApiFactory;
using AicAms.Helpers;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Models.Department;
using AicAms.Models.Surprise;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using Xamarin.Forms;

namespace AicAms.ViewModels.Details
{
    public class SurpriseResultViewModel : ObservableObject, ICancellable
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private readonly User _user;

        private readonly SurpriseFactory _surpFactory;

        public ICommand RefreshCommand { get; }

        public ICommand ShowCommand { get; }

        public ICommand ResendCommand { get; }

        public ObservableCollection<MasterSurprise> MasterSurprises { get; set; } = new ObservableCollection<MasterSurprise>();

        public ObservableCollection<DetailSurprise> DetailSurprises { get; set; } = new ObservableCollection<DetailSurprise>();

        public Color FirstColor => Color.FromRgb(254, 255, 252);

        public Color SecondColor => Color.FromRgb(219, 216, 220);

        public bool IsFooterVisible => DetailSurprises.Any(e => !e.IsChecked);

        private MasterSurprise _selectecSurprise = null;

        public MasterSurprise SelectedSurprise
        {
            get { return _selectecSurprise; }
            set { SetProperty(ref _selectecSurprise, value); }
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

        public bool IsGridTitleVisible => DetailSurprises.Count > 0;

        private string _masterSurprisePikerTitle = Resource.MasterSurpriseTitle;

        public string MasterSurprisePikerTitle
        {
            get { return _masterSurprisePikerTitle; }
            set { SetProperty(ref _masterSurprisePikerTitle, value); }
        }

        private bool _isBlockingDisplay;

        public bool IsBlockingDisplay
        {
            get { return _isBlockingDisplay; }
            set { SetProperty(ref _isBlockingDisplay, value); }
        }

        public SurpriseResultViewModel()
        {
            _user = App.Realm.All<User>().FirstOrDefault();
            if (_user == null)
            {
                NavigationService.SetPage(new LoginViewModel());
                DependencyService.Get<AicAms.DependencyServices.IPushRegister>().Unregister();
                return;
            }
            _surpFactory = new SurpriseFactory();

            RefreshCommand = new Command(Refresh);
            ShowCommand = new Command(Show);
            ResendCommand = new Command(Resend);

            DetailSurprises.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs args)
            {
                OnPropertyChanged(nameof(IsGridTitleVisible));
                OnPropertyChanged(nameof(IsFooterVisible));
            };

            FillMasterSurprises();
        }

        public void CancellAll()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        private async void Resend()
        {
            IsBlockingDisplay = true;

            var res = await _surpFactory.ResendPush(_user.Token, SelectedSurprise.Id, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                MessageViewer.SuccessAsync(Resource.SuccessSentRequest);
            }
            else
            {
                MessageViewer.ErrorAsync(Resource.FailureSentRequst);
            }
            
            IsBlockingDisplay = false;
        }

        private void Show()
        {
            ShowGrid();
        }

        private async Task ShowGrid()
        {
            if (SelectedSurprise == null)
            {
                await MessageViewer.ErrorAsync(Resource.NotEnouthFields);
                return;
            }

            CancellAll();
            DetailSurprises.Clear();
            IsNoDataMsgVisible = false;
            IsIndicatorVisible = true;

            var res = await _surpFactory.GetDetailSurprises(_user.Token, SelectedSurprise.Id, _cancellationToken.Token);

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
                        q.EmpId = q.EmpId.Trim();
                        q.BackgroundColor = i++ % 2 == 0 ? FirstColor : SecondColor;
                        DetailSurprises.Add(q);
                    }
                }
            }
        }

        private async void Refresh()
        {
            CancellAll();
            IsRefreshBusy = true;

            if (DetailSurprises.Count == 0)
            {
                await FillMasterSurprises();
            }
            else if (SelectedSurprise != null)
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

        private async Task FillMasterSurprises()
        {
            CancellAll();
            MasterSurprisePikerTitle = Resource.DownloadingText;
            var res = await _surpFactory.GetMasterSurprises(_user.Token, _cancellationToken.Token);
            if (res.ResultCode == ResultCode.Success)
            {
                SelectedSurprise = null;
                MasterSurprises.Clear();
                foreach (var surprise in res.Data)
                {
                    surprise.Calculate(_user.IsGregorianLocale);
                    MasterSurprises.Add(surprise);
                }
            }
            MasterSurprisePikerTitle = Resource.MasterSurpriseTitle;
        }


    }
}
