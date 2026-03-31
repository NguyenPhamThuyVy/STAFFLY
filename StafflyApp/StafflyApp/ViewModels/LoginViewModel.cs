using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using StafflyApp.Helpers; 
using StafflyApp.Views;   
using System.Windows;
using System.Windows.Controls;

namespace StafflyApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [RelayCommand]
        private void Login(object parameter)
        {
            // Lấy PasswordBox từ parameter truyền sang
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            // Lấy mật khẩu thực tế
            string password = passwordBox.Password;

            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "All fields need to be filled!";
                return;
            }
            // 2. Giả lập kiểm tra Database, sau này sẽ kết nối Database thật
            if (Username == "admin" && password =="123")
            {
                // Lưu thông tin vào UserSession 
                UserSession.UserID = 1;
                UserSession.Username = "Nguyễn Phạm Thúy Vy";
                UserSession.RoleID = 1; // 1 là Admin
                UserSession.RoleName = "Admin";

                // 3. Xử lý chuyển cửa sổ
                // Tìm cửa sổ chứa cái PasswordBox này
                Window currentWindow = Window.GetWindow(passwordBox);
                if (currentWindow != null)
                {
                    // Mở MainWindow
                    MainWindow main = new MainWindow();
                    main.Show();

                    // Đóng cửa sổ Login hiện tại
                    currentWindow.Close();
                }
                else
                {
                    ErrorMessage = "Incorrect username or password!";
                }
            }
        }
    }
}