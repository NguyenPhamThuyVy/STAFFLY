using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StafflyApp.ViewModels
{
    public partial class AdminAccountsViewModel : ObservableObject
    {
        [ObservableProperty] private string _newUsername = string.Empty;
        [ObservableProperty] private string _selectedRole = "Staff";
        [ObservableProperty] private bool _isCreateAccountPopupOpen = false;
        [ObservableProperty] private bool _isEditMode = false;
        [ObservableProperty] private string _newEmail = string.Empty;
        [ObservableProperty] private ObservableCollection<User> _accountsList = new();
        public ObservableCollection<string> RolesCollection { get; set; } = new() { "Staff", "Manager" };

        public AdminAccountsViewModel() => LoadActiveAccounts();

        public void LoadActiveAccounts()
        {
            using (var db = new StafflyDbContext())
            {
                AccountsList = new ObservableCollection<User>(db.Users.OrderBy(u => u.CreatedAt).ToList());
            }
        }

        [RelayCommand]
        private void OpenCreateAccountPopup()
        {
            NewUsername = string.Empty;
            NewEmail = string.Empty;
            IsEditMode = false;
            IsCreateAccountPopupOpen = true;
        }

        [RelayCommand]
        private void ClosePopup() => IsCreateAccountPopupOpen = false;

        [RelayCommand]
        private void CreateAccount(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string rawPassword = passwordBox?.Password ?? "";

            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(rawPassword) || string.IsNullOrWhiteSpace(NewEmail))
            {
                MessageBox.Show("Username, Email, and Password are required!", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    if (db.Users.Any(u => u.Username.ToLower() == NewUsername.ToLower()))
                    {
                        MessageBox.Show("Username already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newUser = new User
                    {
                        Username = NewUsername.Trim(),
                        Email = NewEmail.Trim(),
                        RoleName = SelectedRole,
                        RoleID = SelectedRole == "Manager" ? 2 : 3,
                        Password = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                        IsActive = true,
                        IsDefaultPassword = true, // Force đổi pass
                        CreatedAt = DateTime.Now,
                        IsResetRequested = false,
                        TempPasswordPlain = null
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();
                }

                IsCreateAccountPopupOpen = false;
                NewUsername = string.Empty;
                NewEmail = string.Empty;
                passwordBox.Clear();        
                LoadActiveAccounts();       
                MessageBox.Show("Account created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        [RelayCommand]
        private void ToggleAccountStatus(User user)
        {
            if (user == null) return;
            using (var db = new StafflyDbContext())
            {
                var u = db.Users.Find(user.UserID);
                if (u != null) { u.IsActive = user.IsActive; db.SaveChanges(); }
            }
        }

        [RelayCommand]
        private void ResetPassword(User user)
        {
            if (user == null) return;
            string randomPass = "ST@" + new Random().Next(1000, 9999).ToString();

            using (var db = new StafflyDbContext())
            {
                var u = db.Users.Find(user.UserID);
                if (u != null)
                {
                    u.Password = BCrypt.Net.BCrypt.HashPassword(randomPass);
                    u.IsDefaultPassword = true;
                    u.IsResetRequested = false; 
                    u.TempPasswordPlain = randomPass; 
                    db.SaveChanges();
                    LoadActiveAccounts();
                    MessageBox.Show($"Password reset to: {randomPass}. User will be forced to change it.");
                }
            }
        }
    }
}