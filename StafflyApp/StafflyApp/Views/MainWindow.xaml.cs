using System.Windows;
using System.Windows.Input;
using System.Windows.Media; // Phải có cái này để dùng màu (Brush)
using StafflyApp.Views;

namespace StafflyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Mở lên là nạp Dashboard và tô sáng nút Dashboard luôn
            SetActiveMenu(MenuBorder_Dashboard);
            MainContentArea.Content = new DashboardView();
        }

        // ================= HÀM XỬ LÝ HIỆU ỨNG MENU =================
        private void SetActiveMenu(System.Windows.Controls.Border activeBorder)
        {
            // 1. Tắt đèn: Xóa màu của TẤT CẢ các Menu (cho thành Trong suốt)
            var transparentBrush = new SolidColorBrush(Colors.Transparent);
            MenuBorder_Dashboard.Background = transparentBrush;
            MenuBorder_Employees.Background = transparentBrush;
            MenuBorder_Attendance.Background = transparentBrush;
            MenuBorder_Payroll.Background = transparentBrush;
            MenuBorder_Contracts.Background = transparentBrush;
            MenuBorder_Departments.Background = transparentBrush;

            // 2. Bật đèn: Tô màu Tím nhạt cho Menu đang được chọn
            var activeColor = new BrushConverter().ConvertFrom("#8571FF") as SolidColorBrush;
            activeBorder.Background = activeColor;
        }

        // ================= XỬ LÝ CLICK TỪNG MENU =================
        private void MenuDashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(MenuBorder_Dashboard);
            MainContentArea.Content = new DashboardView();
        }

        private void MenuEmployees_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(MenuBorder_Employees);
            MainContentArea.Content = new EmployeeManagementView();
        }

        // ================= XỬ LÝ HỆ THỐNG =================
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWin = new LoginWindow();
            loginWin.Show();
            this.Close();
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