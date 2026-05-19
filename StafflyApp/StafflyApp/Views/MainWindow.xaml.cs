using StafflyApp.Helpers; // Thêm thư viện để truy xuất UserSession
using StafflyApp.ViewModels;
using System.Windows;
using System.Windows.Input;
using StafflyApp.Views; //
namespace StafflyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Nối ViewModel chính vào DataContext của Window
            this.DataContext = new MainWindowViewModel();
        }

        // --- NHÓM HÀM ĐIỀU HƯỚNG TAB  ---

        private void TabDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.NavigateCommand.Execute("Dashboard");
            }
        }

        private void TabEmployees_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.NavigateCommand.Execute("Employees");
            }
        }

        private void TabDepartments_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.NavigateCommand.Execute("Departments");
            }
        }

        private void TabPayroll_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.NavigateCommand.Execute("Payroll");
            }
        }
        private void TabAttendance_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void TabRecruitment_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void TabContracts_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void TabSystem_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.NavigateCommand.Execute("AdminSettings");
            }
        }

        // --- CÁC HÀM HỆ THỐNG ---

        // ĐÃ THÊM: Logic điều hướng đăng xuất, hủy phiên đăng nhập hiện tại
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out of STAFFLY?",
                                                      "Logout Confirmation",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 1. Xóa sạch thông tin tài khoản lưu vết trong Singleton
                UserSession.Instance.ClearSession();

                // 2. Mở lại màn hình Đăng nhập
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // 3. Đóng cửa sổ quản trị chính hiện tại
                this.Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}