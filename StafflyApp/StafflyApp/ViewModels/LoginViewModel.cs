using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using System.Windows;

namespace StafflyApp.ViewModels
{
    // 1. Phải có partial và kế thừa ObservableObject
    public partial class LoginViewModel : ObservableObject
    {
        private readonly UserRepository _userRepository;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        public LoginViewModel()
        {
            _userRepository = new UserRepository();
        }

        // 2. Hàm Login nằm TRONG ngoặc nhọn của class
        [RelayCommand]
        private void Login()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            User? authenticatedUser = _userRepository.AuthenticateUser(Username, Password);

            if (authenticatedUser != null)
            {
                MessageBox.Show($"Đăng nhập thành công! Chào mừng {authenticatedUser.Username}");
            }
            else
            {
                MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!");
            }
        }
    } // Đóng class
} // Đóng namespace