using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Models;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;
        private readonly DepartmentRepository _deptRepository;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private ObservableCollection<Department> _departments = new();

        [ObservableProperty]
        private Employee? _selectedEmployee;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();
            _deptRepository = new DepartmentRepository();
            LoadData();
        }

        [RelayCommand]
        private void LoadData()
        {
            try
            {
                using (var context = new StafflyDbContext())
                {
                    // EF Core sẽ tự mở kết nối và lấy 15 người cho bạn
                    var list = context.Employees.ToList();
                    Employees = new ObservableCollection<Employee>(list);

                    // Tương tự cho phòng ban (nếu KVy có làm bảng Departments)
                    // var deptList = context.Departments.ToList();
                    // Departments = new ObservableCollection<Department>(deptList);
                }
            }
            catch (Exception ex)
            {
                // Nếu vẫn lỗi, nó sẽ hiện thông báo cho Thịnh biết tại sao
                System.Windows.MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }
        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            // Đã sửa thành FullName và Email khớp với Model của KVy
            var filtered = _repository.GetAllEmployees()
                .Where(e => (e.FullName != null && e.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                            (e.Email != null && e.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Employees = new ObservableCollection<Employee>(filtered);
        }

        [RelayCommand]
        private void Refresh()
        {
            SearchText = string.Empty;
            LoadData();
        }

        [RelayCommand]
        private void DeleteSelected(Employee emp)
        {
            if (emp != null)
            {
                if (_repository.DeleteEmployee(emp.EmployeeID))
                {
                    LoadData();
                }
            }
        }
    }
}