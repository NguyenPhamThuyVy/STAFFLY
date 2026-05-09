using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StafflyApp.Views; // Đảm bảo folder Views chứa PayrollView, EmployeeManagementView...

namespace StafflyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Vừa mở app lên là tự động kích hoạt Tab Dashboard luôn
            TabDashboard_Click(null, null);
        }

        // ================= LOGIC ĐỔI MÀU TAB (ACTIVE / INACTIVE) =================
        private void SetActiveTab(Border activeBorder, TextBlock activeText)
        {
            // 1. Xóa màu tất cả các Tab về trạng thái mặc định (Nền trong suốt, Chữ xám)
            var transparentBrush = new SolidColorBrush(Colors.Transparent);
            var grayTextBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#6B7280");

            Border[] allBorders = { TabDashboard, TabEmployees, TabDepartments, TabAttendance, TabPayroll, TabRecruitment, TabContracts, TabSystem };
            TextBlock[] allTexts = { TextDashboard, TextEmployees, TextDepartments, TextAttendance, TextPayroll, TextRecruitment, TextContracts, TextSystem };

            foreach (var border in allBorders) border.Background = transparentBrush;
            foreach (var text in allTexts)
            {
                text.Foreground = grayTextBrush;
                text.FontWeight = FontWeights.Medium;
            }

            // Riêng chữ của nút System màu đỏ nhạt khi chưa được chọn
            TextSystem.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#E53935");

            // 2. Kích hoạt Tab vừa được Click (Nền xanh dương #1954D1, Chữ trắng, In đậm)
            var activeBgBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1954D1");
            var activeTextBrush = new SolidColorBrush(Colors.White);

            activeBorder.Background = activeBgBrush;
            activeText.Foreground = activeTextBrush;
            activeText.FontWeight = FontWeights.SemiBold;
        }

        // ================= LOGIC CHUYỂN TRANG (ROUTING) =================

        private void TabDashboard_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabDashboard, TextDashboard);
            MainContentArea.Content = new DashboardView();
        }

        private void TabEmployees_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabEmployees, TextEmployees);
            MainContentArea.Content = new EmployeeManagementView();
        }

        private void TabDepartments_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabDepartments, TextDepartments);
            // MainContentArea.Content = new DepartmentView(); 
        }

        private void TabAttendance_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabAttendance, TextAttendance);
            // MainContentArea.Content = new AttendanceView(); 
        }

        private void TabPayroll_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabPayroll, TextPayroll);
            // ĐÃ MỞ KẾT NỐI:
            MainContentArea.Content = new PayrollView();
        }

        private void TabRecruitment_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabRecruitment, TextRecruitment);
            // MainContentArea.Content = new RecruitmentView(); 
        }

        private void TabContracts_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabContracts, TextContracts);
            // MainContentArea.Content = new ContractsView(); 
        }

        private void TabSystem_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveTab(TabSystem, TextSystem);
            // Khi chọn tab System, dùng màu đỏ đặc trưng
            TabSystem.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D32F2F");
            // MainContentArea.Content = new SystemAdminView(); 
        }

        // ================= XỬ LÝ HỆ THỐNG =================

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