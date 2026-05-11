using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private ObservableCollection<Department> _departments = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isDialogOpen = false;

        [ObservableProperty]
        private bool _isTransferMode = false;

        [ObservableProperty]
        private Employee _editingEmployee = new();

        [ObservableProperty]
        private Department? _selectedTargetDept;

        [ObservableProperty]
        private string _formTitle = "ADD EMPLOYEE";

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

                using (var db = new StafflyDbContext())
                {
                    Departments = new ObservableCollection<Department>(db.Departments.ToList());
                }
            }
            catch (Exception ex)
            {
                // Đổi Invoke thành BeginInvoke và bọc trong new Action
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }

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
        private void OpenTransferDialog(Employee emp)
        {
            if (emp == null) return;
            EditingEmployee = emp;
            SelectedTargetDept = null;
            IsTransferMode = true;
            FormTitle = "EMPLOYEE TRANSFER";
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
            bool success = _isEditMode ?
                _repository.UpdateEmployee(EditingEmployee) :
                _repository.AddEmployee(EditingEmployee);

            if (success)
            {
                IsDialogOpen = false;
                LoadData();
            }
            else MessageBox.Show("Operation failed!");
        }

        private void ExecuteTransfer()
        {
            if (SelectedTargetDept == null) return;

            if (SelectedTargetDept.CurrentStaffCount >= SelectedTargetDept.HeadcountLimit)
            {
                MessageBox.Show($"Transfer Denied: {SelectedTargetDept.DepartmentName} is full.",
                                "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        LoadData();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            if (MessageBox.Show($"Delete {emp.FullName}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (_repository.DeleteEmployee(emp.EmployeeID)) LoadData();
            }
        }
    }
}