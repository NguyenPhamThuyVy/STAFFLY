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

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;
        private List<Employee> _allEmployeesMaster = new();

        // =======================================================
        // VÙNG KHAI BÁO BIẾN TRÊN ĐẦU CLASS
        // =======================================================
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

        // QUẢN LÝ PHÂN QUYỀN TRONG HỆ THỐNG
        [ObservableProperty] private bool _isManager;
        [ObservableProperty] private bool _isStaff;
        [ObservableProperty] private bool _canEditDepartment;

        // ĐIỀU KHIỂN OVERLAY SIDE PANEL PROFILE CHI TIẾT
        [ObservableProperty] private bool _isListViewVisible = true;
        [ObservableProperty] private bool _isProfileViewVisible = false;
        [ObservableProperty] private bool _isEditMode = false;
        [ObservableProperty] private bool _isNotEditMode = true;
        [ObservableProperty] private Employee _selectedEmployee = new();
        [ObservableProperty] private ObservableCollection<Employee> _departmentColleagues = new();

        // ĐỒNG BỘ GIÁ TRỊ COMBOBOX LOẠI HỢP ĐỒNG
        [ObservableProperty] private string _selectedContractType = "Full-time";

        // =======================================================
        // CONSTRUCTOR
        // =======================================================
        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();

            var currentUser = StafflyApp.Helpers.UserSession.Instance;
            IsManager = currentUser.RoleID == 2;
            IsStaff = currentUser.RoleID == 3;

            _ = LoadData();
        }

        // =======================================================
        // CÁC PHƯƠNG THỨC LOGIC DỮ LIỆU
        // =======================================================
        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                var list = await Task.Run(() => _repository.GetAllEmployees());
                _allEmployeesMaster = list.Where(e => string.IsNullOrEmpty(e.Status) || !e.Status.Equals("Resigned", StringComparison.OrdinalIgnoreCase)).ToList();

                TotalEmployees = _allEmployeesMaster.Count;
                ActiveEmployees = _allEmployeesMaster.Count(e => e.Status?.ToUpper() == "ACTIVE" || e.Status == "Working");

                using (var db = new StafflyDbContext())
                {
                    var deptList = await Task.Run(() => db.Departments.ToList());

                    foreach (var emp in _allEmployeesMaster)
                    {
                        var matchingDept = deptList.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                        emp.DepartmentName = matchingDept != null ? matchingDept.DepartmentName : "No Department";
                    }

                    Departments.Clear();
                    foreach (var dept in deptList) Departments.Add(dept);
                }

                Search();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading failed: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchTextChanged(string value) => Search();

        // =======================================================
        // 🔥 ĐÃ SỬA: LOGIC TÌM KIẾM ĐA THÔNG TIN (MULTI-FIELD SEARCH)
        // =======================================================
        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Employees.Clear();
                foreach (var emp in _allEmployeesMaster) Employees.Add(emp);
                return;
            }

            string query = SearchText.Trim();

            // Tìm kiếm không phân biệt hoa thường trên mọi trường thông tin hiển thị
            var filtered = _allEmployeesMaster.Where(e =>
                e.EmployeeID.ToString().Contains(query) ||
                (e.FullName != null && e.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (e.Phone != null && e.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (e.Email != null && e.Email.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (e.DepartmentName != null && e.DepartmentName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (e.Status != null && e.Status.Contains(query, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            Employees.Clear();
            foreach (var emp in filtered) Employees.Add(emp);
        }

        // =======================================================
        // ĐIỀU HƯỚNG VÀ CHỈNH SỬA TRỰC TIẾP TRÊN PANEL PROFILE
        // =======================================================
        [RelayCommand]
        private void ViewProfile(Employee emp)
        {
            if (emp == null) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var fullEmp = db.Employees.FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);
                    if (fullEmp == null) return;

                    var dept = db.Departments.FirstOrDefault(d => d.DepartmentID == fullEmp.DepartmentID);
                    var contract = db.Contracts.FirstOrDefault(c => c.EmployeeID == emp.EmployeeID);

                    fullEmp.DepartmentName = dept?.DepartmentName ?? "No Department";

                    SelectedContractType = contract?.ContractType ?? "Full-time";
                    SelectedEmployee = fullEmp;

                    var colleagues = _allEmployeesMaster.Where(e => e.DepartmentID == fullEmp.DepartmentID && e.EmployeeID != fullEmp.EmployeeID).ToList();
                    DepartmentColleagues = new ObservableCollection<Employee>(colleagues);
                }

                IsEditMode = false;
                IsNotEditMode = true;
                IsProfileViewVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee profile: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void GoBack() => IsProfileViewVisible = false;

        [RelayCommand]
        private void EnableEditMode()
        {
            IsEditMode = true;
            IsNotEditMode = false;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditMode = false;
            IsNotEditMode = true;
            ViewProfile(SelectedEmployee);
        }

        [RelayCommand]
        private async Task SaveChanges()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedEmployee.FullName))
                {
                    MessageBox.Show("Employee name cannot be empty!", "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new StafflyDbContext())
                {
                    db.Employees.Update(SelectedEmployee);

                    var contract = db.Contracts.FirstOrDefault(c => c.EmployeeID == SelectedEmployee.EmployeeID);
                    if (contract == null)
                    {
                        contract = new Contract { EmployeeID = SelectedEmployee.EmployeeID };
                        db.Contracts.Add(contract);
                    }
                    contract.ContractType = SelectedContractType;

                    await db.SaveChangesAsync();
                }

                IsEditMode = false;
                IsNotEditMode = true;
                _ = LoadData();
                MessageBox.Show("Employee details and contract updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save changes failed: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =======================================================
        // DIALOG THÊM MỚI NHÂN VIÊN BAN ĐẦU CỦA STAFF
        // =======================================================
        [RelayCommand]
        private void OpenAddDialog()
        {
            IsTransferMode = false;
            CanEditDepartment = true;
            EditingEmployee = new Employee { Status = "Active", ContractType = "Full-time" };
            FormTitle = "ADD NEW EMPLOYEE";
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void ConfirmAction() => SaveEmployee();

        private void SaveEmployee()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditingEmployee.FullName))
                {
                    MessageBox.Show("Please enter the employee's name!", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EditingEmployee.DepartmentID == null || EditingEmployee.DepartmentID == 0)
                {
                    MessageBox.Show("Please assign a department to this employee!", "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new StafflyDbContext())
                {
                    db.Employees.Add(EditingEmployee);
                    db.SaveChanges();
                }
                IsDialogOpen = false;
                _ = LoadData();
                MessageBox.Show("New employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show("Database Error: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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