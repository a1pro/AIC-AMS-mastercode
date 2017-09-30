using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views.PushActions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SurpriseAccomplishPage : ContentPage
    {
        public SurpriseAccomplishPage(object viewModel)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, true);
            BindingContext = viewModel;
        }
    }
}