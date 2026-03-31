using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Models;
using StafflyApp.Data;
using System.Windows;

namespace StafflyApp.ViewModels
{
    // Sử dụng partial class để Source Generator của CommunityToolkit có thể tự sinh code
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;
        // private readonly DepartmentRepository _deptRepository; // Mở ra nếu cần dùng sau này

        // Property cho danh sách nhân viên hiển thị trên UI
        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        // Property cho nhân viên đang được chọn (nếu có)
        [ObservableProperty]
        private Employee? _selectedEmployee;

        // Property cho nội dung ô tìm kiếm
        [ObservableProperty]
        private string _searchText = string.Empty;

        public EmployeeViewModel()
        {
            // Khởi tạo các Repository
            _repository = new EmployeeRepository();
            // _deptRepository = new DepartmentRepository();

            // Tải dữ liệu lần đầu khi mở trang
            LoadData();
        }

        // --- COMMANDS ---

        [RelayCommand]
        private void LoadData()
        {
            try
            {
                using (var context = new StafflyDbContext())
                {
                    // Lấy danh sách từ database thông qua EF Core
                    var list = context.Employees.ToList();

                    // Cập nhật lên UI
                    Employees = new ObservableCollection<Employee>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu từ Database: {ex.Message}", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Search()
        {
            // Nếu không nhập gì, hiện lại toàn bộ danh sách
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            try
            {
                string searchLower = SearchText.ToLower();

                // Lấy toàn bộ danh sách từ Repository và lọc
                // Lưu ý: FullName và Email phải khớp với Model trong Database của bạn
                var filtered = _repository.GetAllEmployees()
                    .Where(e => (e.FullName != null && e.FullName.ToLower().Contains(searchLower)) ||
                                (e.Email != null && e.Email.ToLower().Contains(searchLower)))
                    .ToList();

                Employees = new ObservableCollection<Employee>(filtered);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            SearchText = string.Empty; // Xóa trắng ô tìm kiếm
            LoadData();               // Tải lại dữ liệu mới nhất
        }

        [RelayCommand]
        private void DeleteSelected(Employee emp)
        {
            if (emp == null) return;

            // Hiển thị xác nhận trước khi xóa (An toàn hơn cho User)
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?",
                                       "Xác nhận xóa",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_repository.DeleteEmployee(emp.EmployeeID))
                    {
                        // Xóa thành công thì tải lại bảng
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa nhân viên này. Vui lòng kiểm tra lại.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}");
                }
            }
        }
    }
}