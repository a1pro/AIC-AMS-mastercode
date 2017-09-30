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
    public partial class MonthSummaryPage : ContentPage
    {
        public MonthSummaryPage(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            (sender as ListView).SelectedItem = null;
        }
    }
}
