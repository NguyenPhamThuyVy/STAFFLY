using System.Windows;
using System.Windows.Controls;
using StafflyApp.ViewModels;

namespace StafflyApp.Views
{
    public partial class AdminAccountsView : UserControl
    {
        public AdminAccountsView()
        {
            InitializeComponent();
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AdminAccountsViewModel vm)
            {
                string rawPassword = NewPasswordBox.Password;

                // Mã hóa BCrypt trước khi gửi xuống ViewModel
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

                //vm.ExecuteCreateAccount(hashedPassword);
                NewPasswordBox.Clear();
            }
        }
    }
}