using AicAms.ViewModels.Details;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Forms;
using Plugin.LocalNotifications;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views.Details
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage(object viewModel)
        {

            try
            {
                InitializeComponent();

               
                UpdateStatus();
                BindingContext = viewModel;

                var model = BindingContext as DashboardViewModel;
                if (model != null)
                    model.UpdateOxyplot += SetPlotView;
            }


            catch(Exception ex)
            {
                DisplayAlert("Alert",ex.Message,"OK");
            }
        }

        public void SetPlotView()
        {
            var plot = new PlotView
            {
                WidthRequest = 250,
                HeightRequest = 250
            };

            plot.SetBinding(PlotView.ModelProperty, new Binding("OxyModel"));

            PlotGrid.Children.Clear();
            PlotGrid.Children.Add(plot);
        }

        public void UpdateStatus()
        {
            try
            {
                pickselectedstatus.Items.Add("en-US");
                pickselectedstatus.Items.Add("ar-SA");
            }
            catch (Exception ex)
            {

            }
        }

        private void Pickerselectedstatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
         
            var SelectedLanuage = pickselectedstatus.SelectedItem;
            AicAms.Models.PassingValue.langauage =Convert.ToString(SelectedLanuage);
        }
    }
}
