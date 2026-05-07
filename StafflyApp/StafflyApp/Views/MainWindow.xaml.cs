using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StafflyApp.Views; // Đảm bảo gọi đúng nơi chứa các UserControl (DashboardView, EmployeeManagementView...)

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
            var grayTextBrush = new BrushConverter().ConvertFrom("#6B7280") as SolidColorBrush;

            // Xóa nền
            TabDashboard.Background = transparentBrush;
            TabEmployees.Background = transparentBrush;
            TabDepartments.Background = transparentBrush;
            TabAttendance.Background = transparentBrush;
            TabPayroll.Background = transparentBrush;
            TabRecruitment.Background = transparentBrush;
            TabContracts.Background = transparentBrush;
            TabSystem.Background = transparentBrush;

            // Đổi chữ về màu xám
            TextDashboard.Foreground = grayTextBrush;
            TextDashboard.FontWeight = FontWeights.Medium;
            TextEmployees.Foreground = grayTextBrush;
            TextEmployees.FontWeight = FontWeights.Medium;
            TextDepartments.Foreground = grayTextBrush;
            TextDepartments.FontWeight = FontWeights.Medium;
            TextAttendance.Foreground = grayTextBrush;
            TextAttendance.FontWeight = FontWeights.Medium;
            TextPayroll.Foreground = grayTextBrush;
            TextPayroll.FontWeight = FontWeights.Medium;
            TextRecruitment.Foreground = grayTextBrush;
            TextRecruitment.FontWeight = FontWeights.Medium;
            TextContracts.Foreground = grayTextBrush;
            TextContracts.FontWeight = FontWeights.Medium;

            // Riêng chữ của nút System màu đỏ nhạt khi chưa được chọn
            TextSystem.Foreground = new BrushConverter().ConvertFrom("#E53935") as SolidColorBrush;
            TextSystem.FontWeight = FontWeights.Medium;

            // 2. Kích hoạt Tab vừa được Click (Nền xanh dương, Chữ trắng, In đậm)
            var activeBgBrush = new BrushConverter().ConvertFrom("#1954D1") as SolidColorBrush;
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
            // Bỏ comment dòng dưới khi bạn đã tạo file DepartmentView.xaml
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
            // MainContentArea.Content = new PayrollView(); 
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
            // Khi chọn tab System, nếu muốn bạn có thể tô nền nó màu Đỏ thay vì xanh dương cho nổi bật bằng lệnh dưới:
            TabSystem.Background = new BrushConverter().ConvertFrom("#D32F2F") as SolidColorBrush;
            // MainContentArea.Content = new SystemAdminView(); 
        }

        // ================= XỬ LÝ HỆ THỐNG =================

        // Tắt ứng dụng
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Cho phép dùng chuột kéo thả cửa sổ (vì WindowStyle="None")
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}