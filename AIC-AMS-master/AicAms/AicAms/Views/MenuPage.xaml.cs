using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            Title = "Menu";
            Icon = Device.OS == TargetPlatform.iOS ? "HamburgerMenu.png" : null;
        }
    }
}
