using System.Windows.Controls;
using StafflyApp.ViewModels;  

namespace StafflyApp.Views
{
    public partial class HRDashboardView : UserControl
    {
        public HRDashboardView()
        {
            InitializeComponent();
            this.DataContext = new DashboardViewModel();
        }
    }
}