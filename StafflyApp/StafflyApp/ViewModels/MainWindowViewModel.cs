using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Helpers;
using System;

namespace StafflyApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty] private object _currentView;
        [ObservableProperty] private string _currentUserInfo = string.Empty;

        // Các cờ phân quyền hiển thị Menu
        [ObservableProperty] private bool _isAdmin;
        [ObservableProperty] private bool _isManager;
        [ObservableProperty] private bool _isStaff;
        [ObservableProperty] private bool _isNotAdmin;

        // THÊM 3 CỜ NÀY ĐỂ ĐIỀU KHIỂN SÁNG TAB MẶC ĐỊNH
        [ObservableProperty] private bool _isDashboardChecked;
        [ObservableProperty] private bool _isEmployeesChecked;
        [ObservableProperty] private bool _isAdminAccountsChecked;

        public MainWindowViewModel()
        {
            var currentUser = UserSession.Instance;
            CurrentUserInfo = $"{currentUser.Username} ({currentUser.RoleName})";

            _isAdmin = currentUser.RoleID == 1;
            _isManager = currentUser.RoleID == 2;
            _isStaff = currentUser.RoleID == 3;
            _isNotAdmin = currentUser.RoleID != 1;

            LoadDefaultModule();
        }

        private void LoadDefaultModule()
        {
            if (_isAdmin)
            {
                IsAdminAccountsChecked = true; // Bật sáng tab Accounts
                Navigate("AdminAccounts");
            }
            else if (_isManager)
            {
                IsDashboardChecked = true; // Bật sáng tab Dashboard
                Navigate("Dashboard");
            }
            else if (_isStaff)
            {
                IsEmployeesChecked = true; // Bật sáng tab Employees
                Navigate("Employees");
            }
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            // ... (Khu vực switch(destination) CỦA BẠN GIỮ NGUYÊN KHÔNG ĐỔI) ...
            switch (destination)
            {
                case "Dashboard": CurrentView = new HRDashboardViewModel(); break;
                case "Employees": CurrentView = new EmployeeViewModel(); break;
                case "Departments": CurrentView = new HRDepartmentViewModel(); break;
                case "Payroll":
                    if (_isManager) CurrentView = new PayrollApprovalViewModel();
                    else if (_isStaff) CurrentView = new PayrollViewModel();
                    break;
                case "AdminAccounts": CurrentView = new AdminAccountsViewModel(); break;
                case "AdminAuditLogs": CurrentView = new AdminAuditLogsViewModel(); break;
                case "AdminDeptLimits": CurrentView = new AdminDeptLimitsViewModel(); break;
            }
        }
    }
}