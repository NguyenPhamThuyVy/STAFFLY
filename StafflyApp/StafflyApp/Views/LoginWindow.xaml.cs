using System;
using System.Windows;
using System.Windows.Input;

namespace StafflyApp.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window // Đã đổi từ UserControl thành Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Hàm này giúp Thịnh có thể nắm chuột kéo cửa sổ di chuyển (vì mình để WindowStyle="None")
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Hàm xử lý khi bấm nút X để đóng app
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Tạm thời thêm hàm cho nút Đăng nhập để test
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Sau này Uyên và Vy làm xong DB thì mình sẽ viết logic check tài khoản ở đây
            MessageBox.Show("Đăng nhập thành công!");

            // Ví dụ: Mở MainWindow sau khi đăng nhập thành công
            // MainWindow main = new MainWindow();
            // main.Show();
            // this.Close();
        }
    }
}