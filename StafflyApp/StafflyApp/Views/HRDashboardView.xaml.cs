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

            // Lắng nghe sự kiện mỗi khi trang Dashboard được load/hiển thị lên màn hình
            this.Loaded += HRDashboardView_Loaded;
        }

        private void HRDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ép DataContext ép kiểu về đúng ViewModel tổng để ra lệnh làm mới dữ liệu từ Database SQL
            if (this.DataContext is HRDashboardViewModel viewModel)
            {
                viewModel.InitializeOrRefresh();
            }
        }
    }
}