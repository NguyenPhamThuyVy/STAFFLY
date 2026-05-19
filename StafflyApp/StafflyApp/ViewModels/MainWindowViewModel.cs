using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Helpers;
using StafflyApp.Views; // Đảm bảo gọi đúng namespace chứa các file XAML (UserControl)

namespace StafflyApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private string _currentUserInfo = string.Empty;

        public MainWindowViewModel()
        {
            // Hiển thị tên và vai trò của người dùng hiện tại lên góc màn hình
            CurrentUserInfo = $"{UserSession.Instance.Username} ({UserSession.Instance.RoleName})";

            // Mặc định vừa mở lên là tự động điều phối vào màn hình chính dựa trên quyền
            LoadDefaultModule();
        }

        private void LoadDefaultModule()
        {
            // Nếu là Admin thì ném vào thẳng màn hình cấu hình hệ thống
            if (UserSession.Instance.RoleID == 1)
            {
                Navigate("AdminSettings");
            }
            else
            {
                Navigate("Dashboard");
            }
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    // Tạm thời cho dùng chung DashboardView vì chưa có file riêng cho HR
                    CurrentView = new StafflyApp.Views.DashboardView();
                    break;

                case "Employees":
                    // ĐÃ SỬA: Gọi đúng tên file thực tế của bạn
                    CurrentView = new StafflyApp.Views.EmployeeManagementView();
                    break;

                case "Departments":
                    CurrentView = new StafflyApp.Views.DepartmentView();
                    break;

                case "Payroll":
                    // Phân quyền chuẩn cho module Bảng Lương
                    if (Helpers.UserSession.Instance.RoleID == 2)
                        CurrentView = new StafflyApp.Views.PayrollApprovalView();
                    else
                        CurrentView = new StafflyApp.Views.PayrollView();
                    break;

                case "AdminSettings":
                    // Tạm thời bỏ qua hoặc ném về Dashboard vì bạn chưa tạo AdminView.xaml
                    // Mở comment dòng dưới ra khi nào bạn tạo file AdminView nhé:
                    // if (Helpers.UserSession.Instance.RoleID == 1) CurrentView = new StafflyApp.Views.AdminView();
                    break;
            }
        }
    }
}