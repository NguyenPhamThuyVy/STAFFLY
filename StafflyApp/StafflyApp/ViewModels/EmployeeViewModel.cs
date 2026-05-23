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
        public List<string> ContractTypes { get; } = new List<string> { "All", "Full-time", "Part-time", "Intern", "Contract" };

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

  
        public EmployeeViewModel()
        {
            _repository = new EmployeeRepository();

            var currentUser = StafflyApp.Helpers.UserSession.Instance;
            _isManager = currentUser.RoleID == 2;
            _isStaff = currentUser.RoleID == 3;

            _ = LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                var list = await Task.Run(() => _repository.GetAllEmployees());
                _allEmployeesMaster = list.Where(e => string.IsNullOrEmpty(e.Status) || !e.Status.Equals("Resigned", StringComparison.OrdinalIgnoreCase)).ToList();

                _totalEmployees = _allEmployeesMaster.Count;
                _activeEmployees = _allEmployeesMaster.Count(e => e.Status?.ToUpper() == "ACTIVE" || e.Status == "Working");

                ApplyFilter();

                using (var db = new StafflyDbContext())
                {
                    var deptList = await Task.Run(() => db.Departments.ToList());
                    _departments.Clear();
                    foreach (var dept in deptList) _departments.Add(dept);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading failed: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            // Bắt đầu với toàn bộ dữ liệu gốc
            var query = _allEmployeesMaster.AsEnumerable();

            // 1. LỌC THEO TỪ KHÓA TÌM KIẾM (Search)
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                query = query.Where(e =>
                    (e.FullName?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    e.EmployeeID.ToString().Contains(_searchText));
            }

            // 2. LỌC THEO PHÒNG BAN (Department)
            // Lấy những phòng ban có thật (bỏ qua giá trị null hoặc phòng ban giả định "Tất cả" nếu có ID = 0)
            if (_selectedFilterDepartment != null && _selectedFilterDepartment.DepartmentID > 0)
            {
                query = query.Where(e => e.DepartmentID == _selectedFilterDepartment.DepartmentID);
            }

            // 3. LỌC THEO LOẠI HỢP ĐỒNG (Contract Type)
            if (!string.IsNullOrEmpty(_selectedFilterContractType) && _selectedFilterContractType != "All")
            {
                // Lưu ý: Đảm bảo Repository của bạn đã Join bảng Contracts để lấy thuộc tính ContractType lên Employee
                query = query.Where(e => e.ContractType == _selectedFilterContractType);
            }

            // ĐỔ DỮ LIỆU LÊN GIAO DIỆN
            var filteredList = query.ToList();
            _employees.Clear();
            foreach (var emp in filteredList)
            {
                _employees.Add(emp);
            }

            // (Tùy chọn) Cập nhật lại con số Total hiển thị trên góc phải màn hình
            // TotalEmployees = filteredList.Count;
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

                    _selectedContractType = contract?.ContractType ?? "Full-time";
                    _selectedEmployee = fullEmp;

                    var colleagues = _allEmployeesMaster.Where(e => e.DepartmentID == fullEmp.DepartmentID && e.EmployeeID != fullEmp.EmployeeID).ToList();
                    _departmentColleagues = new ObservableCollection<Employee>(colleagues);
                }

                _isEditMode = false;
                _isNotEditMode = true;
                _isProfileViewVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee profile: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void GoBack() => _isProfileViewVisible = false;

        [RelayCommand]
        private void EnableEditMode()
        {
            _isEditMode = true;
            _isNotEditMode = false;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            _isEditMode = false;
            _isNotEditMode = true;
            ViewProfile(_selectedEmployee);
        }

        [RelayCommand]
        private async Task SaveChanges()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedEmployee.FullName))
                {
                    MessageBox.Show("Employee name cannot be empty!", "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new StafflyDbContext())
                {
                    db.Employees.Update(_selectedEmployee);

                    var contract = db.Contracts.FirstOrDefault(c => c.EmployeeID == _selectedEmployee.EmployeeID);
                    if (contract == null)
                    {
                        contract = new Contract { EmployeeID = _selectedEmployee.EmployeeID };
                        db.Contracts.Add(contract);
                    }
                    contract.ContractType = _selectedContractType;

                    if (await db.SaveChangesAsync() > 0)
                    {
                        // Ghi log khi chỉnh sửa thông tin nhân viên 
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "UPDATE_EMPLOYEE",
                            $"Updated profile for Employee: '{_selectedEmployee.FullName}' (ID: {_selectedEmployee.EmployeeID}) with contract type: '{_selectedContractType}'.");
                    }
                }

                _isEditMode = false;
                _isNotEditMode = true;
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
            _editingEmployee = _selectedEmployee;
            ExecuteTransfer();
        }

        private void ExecuteTransfer()
        {
            if (_selectedTargetDept == null) return;
            if (_selectedTargetDept.CurrentStaffCount >= _selectedTargetDept.HeadcountLimit)
            {
                MessageBox.Show($"{_selectedTargetDept.DepartmentName} has reached its headcount limit.", "Transfer Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var emp = db.Employees.Find(_editingEmployee.EmployeeID);
                    if (emp != null)
                    {
                        // Lưu vết phòng ban cũ 
                        int oldDeptId = emp.DepartmentID ?? 0;
                        emp.DepartmentID = _selectedTargetDept.DepartmentID;
                        if (db.SaveChanges() > 0)
                        {
                            // Ghi log điều chuyển phòng ban nội bộ
                            UserRepository.LogAction(
                                StafflyApp.Helpers.UserSession.Instance.UserID,
                                "TRANSFER_DEPARTMENT",
                                $"Transferred Employee '{emp.FullName}' (ID: {emp.EmployeeID}) from Dept ID {oldDeptId} to '{_selectedTargetDept.DepartmentName}' (Dept ID: {_selectedTargetDept.DepartmentID})."
                            );
                        }
                        _ = LoadData();

                        _selectedEmployee.DepartmentName = _selectedTargetDept.DepartmentName;
                        OnPropertyChanged(nameof(_selectedEmployee));

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
            _isTransferMode = false;
            _canEditDepartment = true;
            _editingEmployee = new Employee { Status = "Active", ContractType = "Full-time" };
            _formTitle = "ADD NEW EMPLOYEE";
            _isDialogOpen = true;
        }

        [RelayCommand]
        private void ConfirmAction() => SaveEmployee();

        private void SaveEmployee()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_editingEmployee.FullName))
                {
                    MessageBox.Show("Please enter the employee's name!", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_editingEmployee.DepartmentID == null || _editingEmployee.DepartmentID == 0)
                {
                    MessageBox.Show("Please assign a department to this employee!", "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new StafflyDbContext())
                {
                    db.Employees.Add(_editingEmployee);
                    if (db.SaveChanges() > 0)
                    {
                        // Ghi log thêm mới nhân viên
                        UserRepository.LogAction(
                            StafflyApp.Helpers.UserSession.Instance.UserID,
                            "ADD_EMPLOYEE",
                            $"Created new employee profile: '{_editingEmployee.FullName}' assigned to Department ID: {_editingEmployee.DepartmentID}."
                        );
                    }
                }
                _isDialogOpen = false;
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
                // Lưu tên và ID để ghi log
                int empId = emp.EmployeeID;
                string empName = emp.FullName;
                if (_repository.DeleteEmployee(emp.EmployeeID))
                {
                    // Ghi log xóa nhân viên
                    UserRepository.LogAction(
                        StafflyApp.Helpers.UserSession.Instance.UserID,
                        "DELETE_EMPLOYEE",
                        $"Permanently removed employee record: '{empName}' (ID: {empId}) from the operational master list."
                    );
                    _ = LoadData();
                }
            }
        }
    }
}