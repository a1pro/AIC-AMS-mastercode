using AicAms.ViewModels.Details;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AicAms.Views.Details
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            var model = BindingContext as DashboardViewModel;
            if (model != null)
                model.UpdateOxyplot += SetPlotView;
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
    }
}
