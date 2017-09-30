using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RequestInfoPage : PopupPage
    {
        public RequestInfoPage(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}