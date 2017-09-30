using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views.Details
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DaySummaryPage : ContentPage
    {
        public DaySummaryPage(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
