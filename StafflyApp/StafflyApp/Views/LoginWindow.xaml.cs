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

        private void LoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Bước 1: Gọi cửa sổ MainWindow ra
            MainWindow mainWin = new MainWindow();

            // Bước 2: Cho nó hiển thị lên màn hình
            mainWin.Show();

            // Bước 3: Đóng cửa sổ Đăng nhập hiện tại lại
            this.Close();
        }

        // Xử lý nút X (Tắt ứng dụng) trên form Login
        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Xử lý kéo thả cửa sổ khi click giữ vào khoảng trống
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}