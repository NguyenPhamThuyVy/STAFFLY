using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Http.Internal;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StafflyApp.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeRepository _repository;
        private List<Employee> _allEmployeesMaster = new();

        public List<string> ContractTypes { get; } = new List<string> { "All", "Full-time", "Part-time", "Probationary" };
        
        [ObservableProperty] private ObservableCollection<Employee> _employees = new();
        [ObservableProperty] private ObservableCollection<Department> _departments = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private Department _selectedFilterDepartment;
        [ObservableProperty] private string _selectedFilterContractType = "All";
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
                
                // Chạy hàm lọc để tính toán chính xác số lượng đếm ban đầu khi mở tab
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading failed: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Tự động kích hoạt lọc dữ liệu và nhảy số real-time ngay khi gõ phím Search không cần chờ nhấn Enter
        partial void OnSearchTextChanged(string value) => ApplyFilter();

        // =======================================================
        // 🔥 ĐÃ FIX: LOGIC TÌM KIẾM ĐA PHƯƠNG THỨC & TỰ ĐỘNG NHẢY BỘ ĐẾM
        // =======================================================
        [RelayCommand]
        private void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allEmployeesMaster
                : _allEmployeesMaster.Where(e => 
                    e.EmployeeID.ToString().Contains(SearchText.Trim()) ||
                    (e.FullName != null && e.FullName.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (e.Phone != null && e.Phone.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (e.Email != null && e.Email.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (e.DepartmentName != null && e.DepartmentName.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (e.Position != null && e.Position.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)) || 
                    (e.Status != null && e.Status.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase))       
                ).ToList();

            if (SelectedFilterDepartment != null && SelectedFilterDepartment.DepartmentID > 0)
            {
                filtered = filtered.Where(e => e.DepartmentID == SelectedFilterDepartment.DepartmentID).ToList();
            }

            if (!string.IsNullOrEmpty(SelectedFilterContractType) && SelectedFilterContractType != "All")
            {
                filtered = filtered.Where(e => e.ContractType == SelectedFilterContractType).ToList();
            }

            Employees.Clear();
            foreach (var emp in filtered) Employees.Add(emp);

            // 🔥 ĐỒNG BỘ: Sử dụng các thuộc tính Public viết hoa để UI tự động cập nhật số liệu thời gian thực
            TotalEmployees = filtered.Count;
            ActiveEmployees = filtered.Count(e => e.Status?.ToUpper() == "ACTIVE" || e.Status == "Working" || e.Status?.ToUpper() == "PROBATION");
        }

        // =======================================================
        // 🔥 ĐÃ FIX: NẠP VÀ ĐỒNG BỘ ĐẦY ĐỦ POSITION VÀ STARTDATE KHI MỞ PROFILE
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
                    
                    // Gán vào thuộc tính Public để phát tín hiệu cập nhật lên Panel UI kế bên
                    SelectedEmployee = fullEmp;

                    var colleagues = _allEmployeesMaster.Where(e => e.DepartmentID == fullEmp.DepartmentID && e.EmployeeID != fullEmp.EmployeeID).ToList();
                    DepartmentColleagues = new ObservableCollection<Employee>(colleagues);
                }

                // Phát tín hiệu thông báo ép View XAML vẽ lại các trường thông tin chi tiết
                OnPropertyChanged(nameof(SelectedEmployee));

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
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "UPDATE_EMPLOYEE",
                            $"Updated profile for Employee: '{SelectedEmployee.FullName}' (ID: {SelectedEmployee.EmployeeID}) with contract type: '{SelectedContractType}'.");
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
                    var newDept = db.Departments.FirstOrDefault(d => d.DepartmentID == SelectedTargetDept.DepartmentID);
                    var oldDept = db.Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);

                    if (emp == null || newDept == null) return;

                    if (newDept.CurrentStaffCount >= newDept.HeadcountLimit)
                    {
                        MessageBox.Show($"{newDept.DepartmentName} has reached its headcount limit.", "Transfer Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int oldDeptId = emp.DepartmentID ?? 0;
                    string oldDeptName = oldDept != null ? oldDept.DepartmentName : "No Department";
                    emp.DepartmentID = newDept.DepartmentID;

                    newDept.CurrentStaffCount += 1;
                    if (oldDept != null) oldDept.CurrentStaffCount -= 1;

                    if (db.SaveChanges() > 0)
                    {
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "TRANSFER_DEPARTMENT",
                            $"Transferred Employee '{emp.FullName}' (ID: {emp.EmployeeID}) from '{oldDeptName}' to '{newDept.DepartmentName}'."
                        );

                        _ = LoadData();
                        SelectedEmployee.DepartmentName = newDept.DepartmentName;
                        OnPropertyChanged(nameof(SelectedEmployee));

                        MessageBox.Show("Employee transferred successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
            EditingEmployee = new Employee { Status = "Active", ContractType = "Full-time", StartDate = DateTime.Now.Date };
            FormTitle = "ADD NEW EMPLOYEE";
            IsDialogOpen = true;

            OnPropertyChanged(nameof(IsTransferMode));
            OnPropertyChanged(nameof(CanEditDepartment));
            OnPropertyChanged(nameof(EditingEmployee));
            OnPropertyChanged(nameof(FormTitle));
            OnPropertyChanged(nameof(IsDialogOpen));
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

                    if (dept != null && dept.CurrentStaffCount >= dept.HeadcountLimit)
                    {
                        MessageBox.Show($"Save failed: {dept.DepartmentName} has reached its headcount limit ({dept.HeadcountLimit})!",
                                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    db.Employees.Add(EditingEmployee);

                    if (dept != null) dept.CurrentStaffCount += 1;

                    if (db.SaveChanges() > 0)
                    {
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
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                        var dept = db.Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                        if (dept != null && dept.CurrentStaffCount > 0)
                        {
                            dept.CurrentStaffCount -= 1;
                        }

                        int empId = emp.EmployeeID;
                        string empName = emp.FullName;

                        if (_repository.DeleteEmployee(empId))
                        {
                            db.SaveChanges(); 

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