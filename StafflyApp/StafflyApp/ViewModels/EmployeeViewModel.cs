using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;
        private List<Employee> _allEmployeesMaster = new();

        [ObservableProperty] private ObservableCollection<Employee> _employees = new();
        [ObservableProperty] private ObservableCollection<Department> _departments = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private int _totalEmployees;
        [ObservableProperty] private int _activeEmployees;
        [ObservableProperty] private bool _isDialogOpen = false;
        [ObservableProperty] private bool _isTransferMode = false;
        [ObservableProperty] private Employee _editingEmployee = new();
        [ObservableProperty] private Department? _selectedTargetDept;
        [ObservableProperty] private string _formTitle = "ADD EMPLOYEE";
        private bool _isEditMode = false;

        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();
            _ = LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                // Load employees from Repository (ADO.NET)
                var list = await Task.Run(() => _repository.GetAllEmployees());
                _allEmployeesMaster = list.ToList();

                TotalEmployees = _allEmployeesMaster.Count;
                ActiveEmployees = _allEmployeesMaster.Count(e => e.Status?.ToUpper() == "ACTIVE" || e.Status == "Working");

                Search();

                // Load departments from DbContext (Entity Framework)
                using (var db = new StafflyDbContext())
                {
                    var deptList = await Task.Run(() => db.Departments.ToList());
                    Departments.Clear();
                    foreach (var dept in deptList) Departments.Add(dept);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading failed: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchTextChanged(string value) => Search();

        [RelayCommand]
        private void Search()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allEmployeesMaster
                : _allEmployeesMaster.Where(e => (e.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) || e.EmployeeID.ToString().Contains(SearchText)).ToList();

            Employees.Clear();
            foreach (var emp in filtered) Employees.Add(emp);
        }

        [RelayCommand]
        private void OpenAddDialog()
        {
            IsTransferMode = false;
            EditingEmployee = new Employee { Status = "Active" }; // Default status
            FormTitle = "ADD NEW EMPLOYEE";
            _isEditMode = false;
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEditDialog(Employee emp)
        {
            if (emp == null) return;
            IsTransferMode = false;

            // Clone object to avoid direct list modification
            EditingEmployee = new Employee
            {
                EmployeeID = emp.EmployeeID,
                FullName = emp.FullName,
                Email = emp.Email,
                Phone = emp.Phone,
                Address = emp.Address,
                DateOfBirth = emp.DateOfBirth,
                Status = emp.Status,
                DepartmentID = emp.DepartmentID,
                Address = emp.Address,
                DateOfBirth = emp.DateOfBirth
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
            try
            {
                if (string.IsNullOrWhiteSpace(EditingEmployee.FullName))
                {
                    MessageBox.Show("Please enter the employee's name!", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                EditingEmployee.Address ??= "Not updated";
                EditingEmployee.Email ??= "";
                EditingEmployee.Phone ??= "";

                // 4. Department Validation
                if (EditingEmployee.DepartmentID == null || EditingEmployee.DepartmentID == 0)
                {
                    MessageBox.Show("Please assign a department to this employee!", "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 5. Default Values
                if (string.IsNullOrEmpty(EditingEmployee.Address)) EditingEmployee.Address = "Not updated yet";
                if (string.IsNullOrEmpty(EditingEmployee.Status)) EditingEmployee.Status = "Active";

                // 6. Save using Entity Framework
                using (var db = new StafflyDbContext())
                {
                    if (_isEditMode) db.Employees.Update(EditingEmployee);
                    else db.Employees.Add(EditingEmployee);
                    db.SaveChanges();
                }

                IsDialogOpen = false;
                _ = LoadData();
                MessageBox.Show("Employee saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Database Error: {errorMsg}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void ExecuteTransfer()
        {
            if (SelectedTargetDept == null) return;
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
                        MessageBox.Show("Employee transferred successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Transfer Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            if (MessageBox.Show($"Are you sure you want to delete {emp.FullName}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (_repository.DeleteEmployee(emp.EmployeeID)) _ = LoadData();
            }
        }
    }
}