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
    public partial class RequestDecisionPage : ContentPage
    {
        public RequestDecisionPage(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            //EGrid.RaiseChild(EWaitGrid);
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            (sender as ListView).SelectedItem = null;
        }
    }
}