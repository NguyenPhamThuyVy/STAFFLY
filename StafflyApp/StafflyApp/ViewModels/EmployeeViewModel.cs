using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Models;
using StafflyApp.Data;
using System.Windows;
using StafflyApp.Data.Repositories;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isDialogOpen = false;

        [ObservableProperty]
        private Employee _editingEmployee = new();

        [ObservableProperty]
        private string _formTitle = "THÊM NHÂN VIÊN";

        private bool _isEditMode = false;

        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();
            LoadData();
        }

        [RelayCommand]
        public void LoadData()
        {
            try
            {
                var list = _repository.GetAllEmployees();
                Employees = new ObservableCollection<Employee>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        [RelayCommand]
        private void OpenAddDialog()
        {
            EditingEmployee = new Employee();
            FormTitle = "THÊM NHÂN VIÊN MỚI";
            _isEditMode = false;
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEditDialog(Employee emp)
        {
            if (emp == null) return;
            // Tạo bản sao để tránh sửa trực tiếp lên List khi chưa bấm Lưu
            EditingEmployee = new Employee
            {
                EmployeeID = emp.EmployeeID,
                FullName = emp.FullName,
                Email = emp.Email,
                Phone = emp.Phone,
                Status = emp.Status,
                DepartmentID = emp.DepartmentID
            };
            FormTitle = "CẬP NHẬT THÔNG TIN";
            _isEditMode = true;
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void SaveEmployee()
        {
            bool success = _isEditMode ?
                _repository.UpdateEmployee(EditingEmployee) :
                _repository.AddEmployee(EditingEmployee);

            if (success)
            {
                IsDialogOpen = false;
                LoadData();
            }
            else
            {
                MessageBox.Show("Thao tác thất bại!");
            }
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            if (MessageBox.Show($"Xóa {emp.FullName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (_repository.DeleteEmployee(emp.EmployeeID)) LoadData();
            }
        }
    }
}