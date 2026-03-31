using System.Windows.Controls;
using StafflyApp.ViewModels; // Nhớ phải có dòng này trên cùng

namespace StafflyApp.Views
{
    public partial class EmployeeManagementView : UserControl
    {
        // Đây là hàm khởi tạo (Constructor)
        public EmployeeManagementView()
        {
            // Lệnh này do máy tự sinh ra, bắt buộc phải có mặt đầu tiên
            InitializeComponent();

            // Dòng code của mình phải nằm BÊN TRONG hàm này, DƯỚI dòng InitializeComponent()
            this.DataContext = new EmployeeViewModel();
        }
    }
}