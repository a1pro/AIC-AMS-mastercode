using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views.Auth
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            //if (Device.RuntimePlatform == Device.Android)
            //    NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
