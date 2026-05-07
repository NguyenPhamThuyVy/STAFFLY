using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using StafflyApp.Helpers;
using StafflyApp.Views;
using StafflyApp.Data.Repositories; // Đảm bảo dùng đúng namespace Repository
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
            // Lấy PasswordBox từ View truyền sang để lấy mật khẩu bảo mật
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            // 1. Kiểm tra đầu vào trống
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Please enter both username and password!";
                return;
            }

            // 2. Kiểm tra Database thật (Thay vì giả lập admin/123)
            User? authenticatedUser = _userRepository.AuthenticateUser(Username, password);

            if (authenticatedUser != null)
            {
                // 3. Lưu thông tin vào UserSession (Lấy từ dữ liệu thật trong DB)
                UserSession.UserID = authenticatedUser.UserID;
                UserSession.Username = authenticatedUser.Username;
                UserSession.RoleID = authenticatedUser.RoleID ?? 0;
                UserSession.RoleName = authenticatedUser.RoleName;

                // 4. Xử lý chuyển cửa sổ
                Window currentWindow = Window.GetWindow(passwordBox);
                if (currentWindow != null)
                {
                    MainWindow main = new MainWindow();
                    main.Show();
                    currentWindow.Close();
                }
            }
            else
            {
                // Báo lỗi nếu sai tài khoản/mật khẩu
                ErrorMessage = "Incorrect username or password!";
            }
        }
    }
}