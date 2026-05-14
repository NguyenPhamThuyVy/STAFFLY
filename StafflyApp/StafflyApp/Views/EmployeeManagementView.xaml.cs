using StafflyApp.ViewModels; // Nhớ phải có dòng này trên cùng
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StafflyApp.Views
{
    public partial class EmployeeManagementView : UserControl
    {
        // Đây là hàm khởi tạo (Constructor)
        public EmployeeManagementView()
        {           
            InitializeComponent();

            // Dòng code của mình phải nằm BÊN TRONG hàm này, DƯỚI dòng InitializeComponent()
            this.DataContext = new EmployeeViewModel();
        }
        // Thêm đoạn này để xử lý sự kiện nhấn phím
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím F và phím Ctrl được nhấn cùng lúc
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBox.Focus();       // Chuyển con trỏ chuột vào ô Search
                SearchBox.SelectAll();   // Bôi đen toàn bộ chữ cũ (nếu có) để người dùng gõ đè lên
            }
        }
        private void DialogHeader_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Lấy MainWindow chứa cái UserControl này
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.DragMove();
                }
            }
        }
    }
}