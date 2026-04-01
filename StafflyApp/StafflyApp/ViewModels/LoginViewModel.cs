using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using System.Windows;

namespace StafflyApp.ViewModels // Chữ cái đầu nên viết hoa để khớp với XAML
{
    // 1. Phải có partial và kế thừa ObservableObject
    public partial class LoginViewModel : ObservableObject
    {
        private readonly UserRepository _userRepository;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        public LoginViewModel()
        {
            // 1. Khởi tạo context trước
            var context = new StafflyDbContext();

            // 2. Truyền context vào Repository
            _userRepository = new UserRepository(context);
        }

        // 2. RelayCommand (R và C viết hoa) để Toolkit tự sinh ra lệnh LoginCommand
        [RelayCommand]
        private void Login()
        {
            // Các phương thức hệ thống phải viết hoa đúng: string.IsNullOrEmpty, MessageBox.Show
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            // Lưu ý: Toolkit tự sinh ra Property 'Username' (viết hoa) từ field '_username'
            User? authenticatedUser = _userRepository.AuthenticateUser(username, password);

            if (authenticatedUser != null)
            {
                MessageBox.Show($"Đăng nhập thành công! Chào mừng {authenticatedUser.Username}");
                // Sau này bạn có thể thêm logic chuyển màn hình ở đây
            }
            else
            {
                MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!");
            }
        }
    }
}