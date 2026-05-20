using StafflyApp.Helpers;
using StafflyApp.ViewModels;
using StafflyApp.Views;
using System.Windows;
using System.Windows.Input;

namespace StafflyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        // --- HÀM CỦA HR / MANAGER ---
        private void TabDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("Dashboard");
        }

        private void TabEmployees_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("Employees");
        }

        private void TabDepartments_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("Departments");
        }

        private void TabPayroll_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("Payroll");
        }

        // --- HÀM CỦA ADMIN ---
        private void TabAdminAccounts_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("AdminAccounts");
        }

        private void TabAdminAuditLogs_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("AdminAuditLogs");
        }

        private void TabAdminDeptLimits_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
                vm.NavigateCommand.Execute("AdminDeptLimits");
        }

        // --- CÁC HÀM HỆ THỐNG ---
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out of STAFFLY?",
                                                      "Logout Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                UserSession.Instance.ClearSession();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
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