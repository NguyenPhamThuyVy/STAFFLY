using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;

        // --- PROPERTIES ---
        [ObservableProperty] private ObservableCollection<Employee> _employees = new();
        [ObservableProperty] private ObservableCollection<Department> _departments = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _isDialogOpen = false;
        [ObservableProperty] private bool _isTransferMode = false;
        [ObservableProperty] private Employee _editingEmployee = new();
        [ObservableProperty] private Department? _selectedTargetDept;
        [ObservableProperty] private string _formTitle = "ADD EMPLOYEE";
        private bool _isEditMode = false;

        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();
            // Tải dữ liệu khi khởi tạo
            _ = LoadData();
        }

        // --- DATA LOADING ---
        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                // Chạy truy vấn ở luồng ngầm để giao diện không bị khựng
                var list = await Task.Run(() => _repository.GetAllEmployees());

                Employees = new ObservableCollection<Employee>(list);

                using (var db = new StafflyDbContext())
                {
                    var deptList = await Task.Run(() => db.Departments.ToList());
                    Departments = new ObservableCollection<Department>(deptList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading failed: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- DIALOG COMMANDS (ADD / EDIT) ---
        [RelayCommand]
        private void OpenAddDialog()
        {
            IsTransferMode = false;
            EditingEmployee = new Employee();
            FormTitle = "ADD NEW EMPLOYEE";
            _isEditMode = false;
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEditDialog(Employee emp)
        {
            if (emp == null) return;
            IsTransferMode = false;
            EditingEmployee = new Employee
            {
                EmployeeID = emp.EmployeeID,
                FullName = emp.FullName,
                Email = emp.Email,
                Phone = emp.Phone,
                Status = emp.Status,
                DepartmentID = emp.DepartmentID
            };
            FormTitle = "UPDATE INFORMATION";
            _isEditMode = true;
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void ConfirmAction()
        {
            if (IsTransferMode) ExecuteTransfer();
            else SaveEmployee();
        }

        private void SaveEmployee()
        {
            // Simple Validation
            if (string.IsNullOrWhiteSpace(EditingEmployee.FullName))
            {
                MessageBox.Show("Employee name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _isEditMode ?
                _repository.UpdateEmployee(EditingEmployee) :
                _repository.AddEmployee(EditingEmployee);

            if (success)
            {
                IsDialogOpen = false;
                _ = LoadData();
            }
            else
            {
                MessageBox.Show("Operation failed!", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- TRANSFER LOGIC ---
        [RelayCommand]
        private void OpenTransferDialog(Employee emp)
        {
            if (emp == null) return;
            EditingEmployee = emp;
            SelectedTargetDept = null;
            IsTransferMode = true;
            FormTitle = "EMPLOYEE TRANSFER";
            IsDialogOpen = true;
        }

        private void ExecuteTransfer()
        {
            if (SelectedTargetDept == null) return;

            // Kiểm tra định biên phòng ban
            if (SelectedTargetDept.CurrentStaffCount >= SelectedTargetDept.HeadcountLimit)
            {
                MessageBox.Show($"{SelectedTargetDept.DepartmentName} has reached its headcount limit.", "Transfer Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var emp = db.Employees.Find(EditingEmployee.EmployeeID);
                    if (emp != null)
                    {
                        emp.DepartmentID = SelectedTargetDept.DepartmentID;
                        db.SaveChanges();
                        IsDialogOpen = false;
                        _ = LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transfer error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- DELETE LOGIC ---
        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            if (MessageBox.Show($"Are you sure you want to delete {emp.FullName}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (_repository.DeleteEmployee(emp.EmployeeID)) _ = LoadData();
            }
        }
    }
}