using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AicAms.Helpers;
using AicAms.Models.Reports;
using AicAms.Services;
using Xamarin.Forms;

namespace AicAms.ViewModels.Popups
{
    public class RequestInfoViewModel : ObservableObject
    {
        public ReportRequest Request { get; }

        public bool IsVac => Request.RetId == "VAC";

        public bool IsExc => !IsVac;

        public ICommand BackCommand { get; }

        public RequestInfoViewModel(ReportRequest request)
        {
            Request = request;
            BackCommand = new Command(Back);
        }

        private async void Back()
        {
            await NavigationService.BackPopup();
        }
    }
}
