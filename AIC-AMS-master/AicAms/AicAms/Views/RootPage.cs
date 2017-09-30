using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Models;
using AicAms.Services;
using AicAms.ViewModels;
using AicAms.ViewModels.Auth;
using AicAms.Views.Auth;
using Xamarin.Forms;

namespace AicAms.Views
{
    public class RootPage : MasterDetailPage
    {
        private readonly MenuViewModel _masterContext;

        public RootPage()
        {
            _masterContext = new MenuViewModel();
            Master = new MenuPage()
            {
                BindingContext = _masterContext
            };
            MasterBehavior = MasterBehavior.Popover;
        }

        public void ConfirmMenuOptions(SelectedMenuOptions options)
        {
            _masterContext.ConfirmMenuOptions(options);
        }
    }
}
