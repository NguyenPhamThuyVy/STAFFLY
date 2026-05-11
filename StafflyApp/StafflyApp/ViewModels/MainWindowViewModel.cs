using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StafflyApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        public MainWindowViewModel()
        {
            // Mặc định vừa mở lên là vào Dashboard
            Navigate("Dashboard");
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    CurrentView = new DashboardViewModel();
                    break;
                case "Employees":
                    CurrentView = new EmployeeViewModel();
                    break;
                case "Departments":
                    CurrentView = new DepartmentViewModel();
                    break;
                case "Payroll":
                    CurrentView = new PayrollViewModel();
                    break;
                    // Bạn có thể thêm các trang khác sau này ở đây
            }
        }
    }
}