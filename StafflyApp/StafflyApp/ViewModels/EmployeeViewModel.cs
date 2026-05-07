using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Models;
using StafflyApp.Data.Repositories;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;
        private readonly DepartmentRepository _deptRepository;

        // --- Data Properties (Vy's logic) ---
        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private ObservableCollection<Department> _departments = new();

        [ObservableProperty]
        private Employee? _selectedEmployee;

        [ObservableProperty]
        private string _searchText = string.Empty;

        // --- UI Dialog & Form Properties (Thinh's UI) ---
        [ObservableProperty]
        private bool _isDialogOpen = false;

        [ObservableProperty]
        private Employee _editingEmployee = new();

        [ObservableProperty]
        private string _formTitle = "ADD EMPLOYEE";

        private bool _isEditMode = false;

        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();
            _deptRepository = new DepartmentRepository();
            LoadData();
        }

        [RelayCommand]
        public void LoadData()
        {
            try
            {
                var list = _repository.GetAllEmployees();
                Employees = new ObservableCollection<Employee>(list);

                var deptList = _deptRepository.GetAllDepartments();
                Departments = new ObservableCollection<Department>(deptList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Dialog Actions (Thinh's UI Logic) ---
        [RelayCommand]
        private void OpenAddDialog()
        {
            EditingEmployee = new Employee();
            FormTitle = "ADD NEW EMPLOYEE";
            _isEditMode = false;
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEditDialog(Employee emp)
        {
            var target = emp ?? SelectedEmployee;
            if (target == null) return;

            EditingEmployee = new Employee
            {
                EmployeeID = target.EmployeeID,
                FullName = target.FullName,
                Email = target.Email,
                Phone = target.Phone,
                Status = target.Status,
                DepartmentID = target.DepartmentID,
                Address = target.Address,
                DateOfBirth = target.DateOfBirth
            };
            FormTitle = "EDIT EMPLOYEE INFORMATION";
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
                MessageBox.Show("Operation failed! Please check your data.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // --- Search & Utilities (Vy's logic) ---
        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }
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
        private void DeleteEmployee(Employee emp)
        {
            var target = emp ?? SelectedEmployee;
            if (target == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {target.FullName}?",
                                       "Confirm Delete",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_repository.DeleteEmployee(target.EmployeeID))
                    LoadData();
            }
        }
    }
}