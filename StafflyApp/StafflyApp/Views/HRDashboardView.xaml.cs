using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using StafflyApp.ViewModels;

namespace StafflyApp.Views
{
    public partial class HRDashboardView : UserControl
    {
        public HRDashboardView()
        {
            InitializeComponent();
            this.Loaded += HRDashboardView_Loaded;
            this.DataContext = new DashboardViewModel();
        }
        private void HRDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is HRDashboardViewModel viewModel)
            {
                viewModel.InitializeOrRefresh();
            }
        } 
    }
}
