using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Models;
using StafflyApp.Data.Repositories; // Đảm bảo dùng đúng namespace Repository

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
            // Khởi tạo các repository đã gộp ở bước trước
            _repository = new EmployeeRepository();
            _deptRepository = new DepartmentRepository();
            LoadData();
        }

        [RelayCommand]
        private void LoadData()
        {
            try
            {
                // Dùng Repository thay vì gọi DbContext trực tiếp để đúng mô hình MVVM
                var list = _repository.GetAllEmployees();
                Employees = new ObservableCollection<Employee>(list);

                var deptList = _deptRepository.GetAllDepartments();
                Departments = new ObservableCollection<Department>(deptList);
            }
            catch (Exception ex)
            {
                // Giữ lại phần thông báo lỗi của Current để dễ debug
                System.Windows.MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
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

            // Sử dụng hàm SearchEmployees xịn mà Vy vừa thêm vào Repository lúc nãy
            var filtered = _repository.SearchEmployees(SearchText);
            Employees = new ObservableCollection<Employee>(filtered);
        }

        [RelayCommand]
        private void Refresh()
        {
            SearchText = string.Empty;
            LoadData();
        }

        [RelayCommand]
        private void AddNewEmployee()
        {
            // Logic refresh sau khi thêm (giữ từ Incoming)
            LoadData();
        }

        [RelayCommand]
        private void DeleteSelected(Employee emp)
        {
            // Ưu tiên dùng tham số emp truyền từ UI vào cho chính xác
            var target = emp ?? SelectedEmployee;
            if (target != null)
            {
                if (_repository.DeleteEmployee(target.EmployeeID))
                {
                    LoadData();
                }
            }
        }
    }
}