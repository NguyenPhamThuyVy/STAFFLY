using System.Windows;
using System.Windows.Input;
using StafflyApp.ViewModels;

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
        }

        // --- CÁC HÀM HỆ THỐNG ---

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