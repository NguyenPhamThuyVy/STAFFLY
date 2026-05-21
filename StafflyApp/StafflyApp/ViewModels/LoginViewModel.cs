using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Models;
using StafflyApp.Helpers;
using StafflyApp.Views;
using StafflyApp.Data.Repositories;
using System.Windows;
using System.Windows.Controls;

namespace StafflyApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly UserRepository _userRepository;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel()
        {
            // Khởi tạo Repository để kết nối Database thật
            var context = new StafflyDbContext();
            _userRepository = new UserRepository(context);
        }

        [RelayCommand]
        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Please enter both username and password!";
                return;
            }

            User? authenticatedUser = _userRepository.AuthenticateUser(Username, password);

            if (authenticatedUser != null)
            {
                // Kiểm tra tài khoản có bị Admin VÔ HIỆU HÓA (Disabled) hay không
                if (!authenticatedUser.IsActive)
                {
                    ErrorMessage = "Your account has been deactivated. Please contact Admin!";
                    MessageBox.Show("This account is currently disabled by the Administrator.",
                                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                // Gán thông tin người dùng vào Session hệ thống
                UserSession.Instance.UserID = authenticatedUser.UserID;
                UserSession.Instance.Username = authenticatedUser.Username;
                UserSession.Instance.RoleID = authenticatedUser.RoleID ?? 0;
                UserSession.Instance.RoleName = authenticatedUser.RoleName;

                // Tìm cửa sổ đăng nhập hiện tại để đóng sau khi chuyển trang
                Window currentWindow = Window.GetWindow(passwordBox);

                // Kiểm tra nếu đang xài mật khẩu mặc định/mật khẩu vừa được reset
                if (authenticatedUser.IsDefaultPassword)
                {
                    MessageBox.Show("Your account is using a temporary or default password.\nYou MUST change your password immediately before accessing the system!",
                                    "Security Requirement", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Điều hướng sang cửa sổ Đổi mật khẩu 
                    ChangePasswordWindow changePassWin = new ChangePasswordWindow();
                    changePassWin.Show();

                    if (currentWindow != null)
                    {
                        currentWindow.Close(); // Đóng cửa sổ Login cũ đi
                    }
                    return; // Chặn đứng luồng chạy, không cho xuống đoạn mở MainWindow bên dưới
                }

                // LUỒNG CHẠY BÌNH THƯỜNG: Nếu mật khẩu an toàn, mở thẳng Dashboard chính
                if (currentWindow != null)
                {
                    MainWindow main = new MainWindow();
                    main.Show();
                    currentWindow.Close();
                }
            }
            else
            {
                ErrorMessage = "Incorrect username or password!";
            }
        }
    }
}