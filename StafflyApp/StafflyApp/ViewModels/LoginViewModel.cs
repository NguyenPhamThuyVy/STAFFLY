using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
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
                // Gán vào Singleton mới
                UserSession.Instance.UserID = authenticatedUser.UserID;
                UserSession.Instance.Username = authenticatedUser.Username;
                UserSession.Instance.RoleID = authenticatedUser.RoleID ?? 0;
                UserSession.Instance.RoleName = authenticatedUser.RoleName;

                // Xử lý chuyển cửa sổ 
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
                ErrorMessage = "Incorrect username or password!";
            }
        }
    }
}