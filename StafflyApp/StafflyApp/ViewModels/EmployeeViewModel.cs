using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
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

        // Danh mục chuẩn hiển thị trên ComboBox lọc hợp đồng
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
                    var contractList = await Task.Run(() => db.Contracts.ToList());
                    foreach (var emp in _allEmployeesMaster)
                    {
                        var matchingDept = deptList.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                        emp.DepartmentName = matchingDept != null ? matchingDept.DepartmentName : "No Department";
                        var matchingContract = contractList.FirstOrDefault(c => c.EmployeeID == emp.EmployeeID);
                        emp.ContractType = matchingContract != null ? matchingContract.ContractType : "Full-time";
                    }
                    Departments.Clear();
                    foreach (var dept in deptList) Departments.Add(dept);
                }
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading failed: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnSelectedFilterDepartmentChanged(Department value) => ApplyFilter();
        partial void OnSelectedFilterContractTypeChanged(string value) => ApplyFilter();

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

            if (!string.IsNullOrEmpty(SelectedFilterContractType) && !SelectedFilterContractType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(e => {
                    if (string.IsNullOrEmpty(e.ContractType)) return false;
                    string targetFilter = SelectedFilterContractType.Replace("ary", "");
                    return e.ContractType.Contains(targetFilter, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }

            // Đồng bộ bộ đếm danh sách
            Employees.Clear();
            foreach (var emp in filtered) Employees.Add(emp);

            TotalEmployees = filtered.Count;
            ActiveEmployees = filtered.Count(e => e.Status?.ToUpper() == "ACTIVE" || e.Status == "Working" || e.Status?.ToUpper() == "PROBATION");
        }
        // ===================================================================
        // RESET TOÀN BỘ BỘ LỌC VỀ TRẠNG THÁI BAN ĐẦU
        // Xóa sạch chữ tìm kiếm, trả ComboBox về "All" và hiển thị lại toàn bộ nhân sự
        // ===================================================================
        [RelayCommand]
        private void ResetFilter()
        {
            // 1. Giải phóng chuỗi văn bản gõ tìm kiếm
            SearchText = string.Empty;

            // 2. Reset ComboBox phòng ban về trạng thái không chọn (Null)
            SelectedFilterDepartment = null;

            // 3. Đưa ComboBox loại hợp đồng về mặc định hiển thị tất cả
            SelectedFilterContractType = "All";

            // 4. Gọi lại hàm ApplyFilter() để ép ứng dụng nạp lại danh sách Master tổng
            ApplyFilter();
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

        // ===================================================================
        // ĐỒNG BỘ LƯU HỢP ĐỒNG SANG BẢNG CONTRACTS KHI TẠO MỚI
        // ===================================================================
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
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var dept = db.Departments.FirstOrDefault(d => d.DepartmentID == EditingEmployee.DepartmentID);

                            if (dept != null && dept.CurrentStaffCount >= dept.HeadcountLimit)
                            {
                                MessageBox.Show($"Save failed: {dept.DepartmentName} has reached its headcount limit ({dept.HeadcountLimit})!",
                                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            EditingEmployee.Department = null;

                            string selectedContractFromForm = EditingEmployee.ContractType ?? "Full-time";

                            db.Employees.Add(EditingEmployee);
                            if (dept != null) dept.CurrentStaffCount += 1;
                            db.SaveChanges();

                            var newContract = new Contract
                            {
                                EmployeeID = EditingEmployee.EmployeeID,
                                ContractType = selectedContractFromForm,
                                BasicSalary = 5000000,
                                SignDate = DateTime.Now.Date
                            };
                            db.Contracts.Add(newContract);
                            db.SaveChanges();

                            transaction.Commit();

                            UserRepository.LogAction(
                                StafflyApp.Helpers.UserSession.Instance.UserID,
                                "ADD_EMPLOYEE",
                                $"Created new employee: '{EditingEmployee.FullName}' (ID: {EditingEmployee.EmployeeID})."
                            );

                            IsDialogOpen = false;
                            _ = LoadData();
                            MessageBox.Show("New employee added successfully with their contract initialized!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (DbUpdateException ex)
                        {
                            transaction.Rollback();
                            HandleDatabaseError(ex);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("System error: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical error: " + ex.Message);
            }
        }

        private void HandleDatabaseError(DbUpdateException ex)
        {
            // Lấy thông báo lỗi chi tiết nhất từ SQL
            string msg = ex.InnerException?.Message ?? ex.Message;

            // Thay vì tìm chữ UNIQUE KEY, ta tìm trực tiếp tên Index đã đặt
            if (msg.Contains("IX_Employees_Email") || msg.Contains("IX_Employees_Phone"))
            {
                List<string> conflicts = new List<string>();
                if (msg.Contains("IX_Employees_Email")) conflicts.Add("Email");
                if (msg.Contains("IX_Employees_Phone")) conflicts.Add("Phone Number");

                string errorMessage = string.Join(" and ", conflicts) + " already exists in the system.";
                MessageBox.Show(errorMessage, "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Nếu không phải lỗi trùng, hiện lỗi gốc để debug
                MessageBox.Show("Database error: " + msg, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            if (!string.IsNullOrEmpty(emp.Status) && (emp.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || emp.Status.Equals("Working", StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Cannot delete '{emp.FullName}' because their current status is marked as [{emp.Status}]!\n\n" +
                                "To remove this record, you must change their status to 'Inactive' or 'Resigned' in their Profile settings first.",
                                "Action Prohibited", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to permanently delete {emp.FullName} from the master database?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new StafflyDbContext())
                    {
                        var employeeInDb = db.Employees.FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);
                        if (employeeInDb != null)
                        {
                            // Tìm phòng ban tương ứng của nhân viên đó để trừ bộ đếm số lượng
                            var dept = db.Departments.FirstOrDefault(d => d.DepartmentID == employeeInDb.DepartmentID);
                            if (dept != null && dept.CurrentStaffCount > 0)
                            {
                                dept.CurrentStaffCount -= 1; // Giảm định biên phòng ban thực tế xuống 1 đơn vị
                            }

                            int empId = employeeInDb.EmployeeID;
                            string empName = employeeInDb.FullName;

                            // Thực hiện xóa dòng nhân viên ra khỏi bảng ngay trong cùng kết nối cục bộ
                            db.Employees.Remove(employeeInDb);

                            // Thực hiện lưu đồng loạt cả hành vi XÓA NHÂN VIÊN và CẬP NHẬT PHÒNG BAN xuống SQL Server
                            db.SaveChanges();

                            // 3. Ghi Nhật ký Hệ thống (Audit Logs)
                            UserRepository.LogAction(
                                StafflyApp.Helpers.UserSession.Instance.UserID,
                                "DELETE_EMPLOYEE",
                                $"Permanently removed employee record: '{empName}' (ID: {empId}) from the operational master list."
                            );

                            // 4. Làm mới lại danh sách DataGrid trên giao diện
                            _ = LoadData();
                            MessageBox.Show("Employee deleted and department headcount updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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