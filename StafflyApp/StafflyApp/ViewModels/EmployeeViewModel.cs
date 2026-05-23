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

        [ObservableProperty] private bool _isManager;
        [ObservableProperty] private bool _isStaff;
        [ObservableProperty] private bool _canEditDepartment;

        [ObservableProperty] private bool _isListViewVisible = true;
        [ObservableProperty] private bool _isProfileViewVisible = false;
        [ObservableProperty] private bool _isEditMode = false;
        [ObservableProperty] private bool _isNotEditMode = true;
        [ObservableProperty] private Employee _selectedEmployee = new();
        [ObservableProperty] private ObservableCollection<Employee> _departmentColleagues = new();


        [ObservableProperty] private string _selectedContractType = "Full-time";

        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();

            var currentUser = StafflyApp.Helpers.UserSession.Instance;
            IsManager = currentUser.RoleID == 2;
            IsStaff = currentUser.RoleID == 3;

            _ = LoadData();
        }

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

                    // Đổ tên phòng ban động chuẩn cấu trúc của Vy
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

                    if (await db.SaveChangesAsync() > 0)
                    {
                        // Ghi log sửa thông tin nhân viên 
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "UPDATE_EMPLOYEE",
                            $"Updated profile for Employee: '{SelectedEmployee.FullName}' (ID: {SelectedEmployee.EmployeeID}) with contract type: '{SelectedContractType}'."
                        );
                    }
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

        [RelayCommand]
        private void ConfirmTransfer()
        {
            EditingEmployee = SelectedEmployee;
            ExecuteTransfer();
        }

        private void ExecuteTransfer()
        {
            if (SelectedTargetDept == null) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var emp = db.Employees.Find(EditingEmployee.EmployeeID);
                    var oldDept = db.Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                    var newDept = db.Departments.FirstOrDefault(d => d.DepartmentID == SelectedTargetDept.DepartmentID);

                    if (emp == null || newDept == null) return;

                    // Chặn nếu phòng ban đích đã đầy giới hạn
                    if (newDept.CurrentStaffCount >= newDept.HeadcountLimit)
                    {
                        MessageBox.Show($"{newDept.DepartmentName} has reached its headcount limit.", "Transfer Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int oldDeptId = emp.DepartmentID ?? 0;
                    emp.DepartmentID = newDept.DepartmentID;

                    // Tăng số lượng phòng mới, giảm phòng cũ
                    newDept.CurrentStaffCount += 1;
                    if (oldDept != null) oldDept.CurrentStaffCount -= 1;

                    if (db.SaveChanges() > 0)
                    {
                        // Ghi log điều chuyển phòng ban 
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "TRANSFER_DEPARTMENT",
                            $"Transferred Employee '{emp.FullName}' (ID: {emp.EmployeeID}) from Dept ID {oldDeptId} to '{SelectedTargetDept.DepartmentName}' (Dept ID: {SelectedTargetDept.DepartmentID})."
                        );
                    }

                    _ = LoadData();
                    SelectedEmployee.DepartmentName = newDept.DepartmentName;
                    OnPropertyChanged(nameof(SelectedEmployee));

                    MessageBox.Show("Employee transferred successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transfer Error: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
                    var dept = db.Departments.FirstOrDefault(d => d.DepartmentID == EditingEmployee.DepartmentID);

                    // Chặn thêm mới nếu phòng ban được chọn đã hết chỗ
                    if (dept != null && dept.CurrentStaffCount >= dept.HeadcountLimit)
                    {
                        MessageBox.Show($"Save failed: {dept.DepartmentName} has reached its headcount limit ({dept.HeadcountLimit})!",
                                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    db.Employees.Add(EditingEmployee);

                    // Cộng dồn nhân sự phòng ban
                    if (dept != null) dept.CurrentStaffCount += 1;

                    if (db.SaveChanges() > 0)
                    {
                        // Ghi log 
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "ADD_EMPLOYEE",
                            $"Created new employee profile: '{EditingEmployee.FullName}' assigned to Department ID: {EditingEmployee.DepartmentID}."
                        );
                    }
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
                try
                {
                    using (var db = new StafflyDbContext())
                    {
                        // Tìm phòng ban để trừ số lượng nhân viên hiện tại xuống
                        var dept = db.Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                        if (dept != null && dept.CurrentStaffCount > 0)
                        {
                            dept.CurrentStaffCount -= 1;
                        }

                        int empId = emp.EmployeeID;
                        string empName = emp.FullName;

                        if (_repository.DeleteEmployee(empId))
                        {
                            db.SaveChanges(); // Lưu cập nhật số lượng phòng ban 
                            // Ghi log
                            UserRepository.LogAction(
                                StafflyApp.Helpers.UserSession.Instance.UserID,
                                "DELETE_EMPLOYEE",
                                $"Permanently removed employee record: '{empName}' (ID: {empId}) from the operational master list."
                            );

                            _ = LoadData();
                            MessageBox.Show("Employee deleted and department count updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete Error: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}