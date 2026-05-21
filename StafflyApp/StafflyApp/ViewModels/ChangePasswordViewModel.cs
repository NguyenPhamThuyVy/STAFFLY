using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Helpers;
using StafflyApp.Views;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace StafflyApp.ViewModels
{
    public partial class ChangePasswordViewModel : ObservableObject
    {
        [RelayCommand]
        private void ConfirmChangePassword(Window currentWindow)
        {
            // Truy cập các PasswordBox từ View thông qua thuộc tính 'currentWindow'
            var passwordBox = currentWindow.FindName("NewPasswordBox") as PasswordBox;
            var confirmBox = currentWindow.FindName("ConfirmPasswordBox") as PasswordBox;

            if (passwordBox == null || confirmBox == null) return;

            string newPass = passwordBox.Password;
            string confirmPass = confirmBox.Password;

            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
            {
                MessageBox.Show("Please enter and confirm your new password.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra mật khẩu khớp nhau
            if (newPass != confirmPass)
            {
                MessageBox.Show("New password and confirmation do not match!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int currentUserId = UserSession.Instance.UserID;

                using (var db = new StafflyDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.UserID == currentUserId);
                    if (user != null)
                    {
                        // Mã hóa mật khẩu mới và gỡ cờ IsDefaultPassword
                        user.Password = BCrypt.Net.BCrypt.HashPassword(newPass);
                        user.IsDefaultPassword = false; // Gỡ cờ bắt buộc đổi mật khẩu

                        db.SaveChanges();

                        MessageBox.Show("Password updated successfully! Welcome to the system.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Mở MainWindow sau khi đổi thành công
                        MainWindow mainWin = new MainWindow();
                        mainWin.Show();

                        currentWindow.Close(); // Đóng cửa sổ đổi mật khẩu
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}