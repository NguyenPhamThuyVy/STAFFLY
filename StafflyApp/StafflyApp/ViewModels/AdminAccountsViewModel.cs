using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StafflyApp.ViewModels
{
    public partial class AdminAccountsViewModel : ObservableObject
    {
        // --- 1. THUỘC TÍNH BINDING FORM ---
        [ObservableProperty] private string _newUsername = string.Empty;
        [ObservableProperty] private string _newEmail = string.Empty; 
        [ObservableProperty] private string _selectedRole = "Staff"; // Mặc định từ ComboBox
        public ObservableCollection<string> RolesCollection { get; set; } = new() { "Staff", "Manager" };

        // Quản lý ẩn hiện Popup
        [ObservableProperty] private bool _isCreateAccountPopupOpen = false;

        // --- 2. DANH SÁCH HIỂN THỊ DATAGRID ---
        [ObservableProperty] private ObservableCollection<User> _accountsList = new();

        public AdminAccountsViewModel()
        {
            LoadActiveAccounts();
        }

        public void LoadActiveAccounts()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var list = db.Users.OrderByDescending(u => u.CreatedAt).ToList();
                    AccountsList = new ObservableCollection<User>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading accounts: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 3. CÁC LỆNH ĐIỀU KHIỂN POPUP ---
        [RelayCommand]
        private void OpenCreateAccountPopup()
        {
            IsCreateAccountPopupOpen = true;
        }

        [RelayCommand]
        private void ClosePopup()
        {
            IsCreateAccountPopupOpen = false;
            NewUsername = string.Empty;
        }

        // --- 4. LOGIC XỬ LÝ SWITCH BẬT / TẮT TRẠNG THÁI (TOGGLE) ---
        [RelayCommand]
        private void ToggleAccountStatus(User user)
        {
            if (user == null) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var userInDb = db.Users.FirstOrDefault(u => u.UserID == user.UserID);
                    if (userInDb != null)
                    {
                        // Lấy trạng thái từ cái nút Toggle gán ngược vào DB
                        userInDb.IsActive = user.IsActive;
                        db.SaveChanges();

                        string msg = user.IsActive ? "ENABLED" : "DISABLED";
                        MessageBox.Show($"Account {user.Username} status updated to: {msg}", "Status Synced", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error toggling status: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 5. HÀM TẠO TÀI KHOẢN (Được gọi từ Sự kiện Click bên file Code Behind) ---
        public bool ExecuteCreateAccount(string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(rawPassword))
            {
                MessageBox.Show("Username and Password cannot be left blank!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    bool isUsernameExist = db.Users.Any(u => u.Username.ToLower() == NewUsername.ToLower());
                    if (isUsernameExist)
                    {
                        MessageBox.Show("This Username is already taken!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    var newUser = new User
                    {
                        Username = NewUsername.Trim(),
                        Email = NewUsername.Trim(),
                        RoleName = SelectedRole,
                        RoleID = SelectedRole == "Manager" ? 2 : 3,
                        Password = BCrypt.Net.BCrypt.HashPassword(rawPassword), 
                        IsActive = true,
                        IsDefaultPassword = true,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(newUser);
                    if (db.SaveChanges() > 0)
                    {
                        MessageBox.Show($"Successfully provisioned credentials for {NewUsername}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsCreateAccountPopupOpen = false; // Đóng popup
                        LoadActiveAccounts(); // Refresh bảng dữ liệu
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during creation: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
        // 6. RESET PASSWORD
        [RelayCommand]
        private void ResetPassword(User user)
        {
            if (user == null) return;

            var confirm = MessageBox.Show($"Do you want to reset the password for account: {user.Username} to default?",
                                          "Reset Password Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.OK) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var userInDb = db.Users.FirstOrDefault(u => u.UserID == user.UserID);
                    if (userInDb != null)
                    {
                        // Sinh mật khẩu tạm ngẫu nhiên dài 6 ký tự
                        string tempPassword = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

                        // Băm bảo mật lưu vào DB, đồng thời bật cờ ép đổi mật khẩu
                        userInDb.Password = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                        userInDb.IsDefaultPassword = true;

                        if (db.SaveChanges() > 0)
                        {
                            // Bắn thông báo mật khẩu tạm cho user
                            MessageBox.Show($"Password reset successful!\n\nTemporary Password for {user.Username} is: [ {tempPassword} ]\n\nPlease copy and give it to the user. They will be forced to change it upon login.",
                                            "Credentials Dispatched", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadActiveAccounts(); // Tải lại bảng
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error resetting password: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}