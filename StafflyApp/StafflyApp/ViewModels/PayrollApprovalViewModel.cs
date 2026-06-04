using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Helpers;
using StafflyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace StafflyApp.ViewModels
{
    public partial class PayrollApprovalViewModel : ObservableObject
    {
        // 🔎 Danh sách các cột lương chi tiết nhân viên hiển thị trên DataGrid
        [ObservableProperty] private ObservableCollection<PayrollImportModel> _pendingPayrolls = new();

        // Danh sách phòng ban đổ vào ComboBox bộ lọc
        [ObservableProperty] private ObservableCollection<Department> _departments = new();

        // Các thuộc tính quản lý bộ lọc (Theo dõi thay đổi để tự động load dữ liệu)
        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;
        [ObservableProperty] private Department? _selectedDepartment;

        // Quản lý trạng thái Popup và nội dung phản hồi lý do từ chối
        [ObservableProperty] private bool _isRejectPopupOpen = false;
        [ObservableProperty] private string _rejectReasonInput = string.Empty;

        // Biến tạm lưu vết bản ghi trạng thái phòng ban đang xử lý phê duyệt
        private DepartmentPayrollStatus? _currentTargetStatus;

        // Quản lý chuỗi thông báo trực quan cho manager
        [ObservableProperty] private string _bannerMessage = string.Empty;
        [ObservableProperty] private bool _hasPendingAlert = false;

        public PayrollApprovalViewModel()
        {
            // Khởi tạo và nạp dữ liệu danh mục phòng ban trước
            LoadInitialData();
            // Quét bảng ghi toàn hệ thống
            AutoDetectFirstPendingSheet();
        }

        private void LoadInitialData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var list = db.Departments.ToList();
                    Departments = new ObservableCollection<Department>(list);
                    SelectedMonth = DateTime.Now.Month;
                    SelectedYear = DateTime.Now.Year;
                    // Chọn mặc định phòng ban đầu tiên nếu có để kích hoạt lọc dữ liệu
                    if (Departments.Any())
                    {
                        SelectedDepartment = Departments.First();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadInitialData Error: " + ex.Message);
            }
        }

        // Tự động kích hoạt tải lại dữ liệu khi người dùng thay đổi bất kỳ bộ lọc nào (Tháng, Năm, Phòng ban)
        partial void OnSelectedMonthChanged(int value) => LoadPendingPayrolls();
        partial void OnSelectedYearChanged(int value) => LoadPendingPayrolls();
        partial void OnSelectedDepartmentChanged(Department? value) => LoadPendingPayrolls();

        /// <summary>
        /// 🔎 CORE LOGIC: Đọc dữ liệu chi tiết từ DB lên DataGrid dựa theo bộ lọc của Manager
        /// </summary>
        private void LoadPendingPayrolls()
        {
            if (SelectedDepartment == null || SelectedMonth < 1 || SelectedMonth > 12 || SelectedYear < 2000)
            {
                PendingPayrolls?.Clear();
                BannerMessage = string.Empty; 
                HasPendingAlert = false;      
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Tìm gói bảng lương của phòng ban đang ở trạng thái 'Pending'
                    _currentTargetStatus = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.DepartmentID == SelectedDepartment.DepartmentID
                                          && s.Month == SelectedMonth
                                          && s.Year == SelectedYear);

                    if (_currentTargetStatus != null && _currentTargetStatus.Status == "Pending")
                    {
                        BannerMessage = $"⚠️ ACTION REQUIRED: This payroll sheet is PENDING and waiting for your approval!";
                        HasPendingAlert = true;

                        // Dùng cú pháp Join trực tiếp từ db.EmployeePayrolls sang db.Employees dựa trên EmployeeID
                        var details = (from ep in db.EmployeePayrolls
                                   join emp in db.Employees on ep.EmployeeID equals emp.EmployeeID
                                   where ep.DepartmentID == SelectedDepartment.DepartmentID // Lọc trực tiếp theo phòng ban của bảng lương
                                      && ep.Month == SelectedMonth
                                      && ep.Year == SelectedYear
                                   select new PayrollImportModel
                                   {
                                       EmployeeID = ep.EmployeeID,
                                       EmployeeName = emp.FullName,
                                       Month = ep.Month,
                                       Year = ep.Year,
                                       BasicSalary = ep.BasicSalary,
                                       TotalBonus = ep.Bonuses,
                                       Deductions = ep.Deductions,
                                       TotalSalary = ep.BasicSalary + ep.Bonuses - ep.Deductions,
                                       IsValid = true,
                                       ErrorNote = string.Empty
                                   }).ToList();

                    PendingPayrolls = new ObservableCollection<PayrollImportModel>(details);
                }
                    else
                    {
                        // Ngược lại nếu là Approved, Rejected hoặc chưa khởi tạo dữ liệu thì ẩn banner
                        PendingPayrolls?.Clear();
                        BannerMessage = string.Empty;
                        HasPendingAlert = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadPendingPayrolls Error: " + ex.Message);
            }
        }
        // Hàm tự động quét bản ghi lương đang chờ được duyệt
        private void AutoDetectFirstPendingSheet()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Quét tìm bảng ghi đầu tiên đang chờ duyệt trên toàn hệ thống
                    var firstPending = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.Status == "Pending");

                    if (firstPending != null)
                    {
                        // 1. Tìm đối tượng phòng ban tương ứng trong danh sách Departments đang có của ViewModel
                        var targetDept = Departments.FirstOrDefault(d => d.DepartmentID == firstPending.DepartmentID);

                        if (targetDept != null)
                        {
                            // 2. Kích hoạt cơ chế tự định vị: Gán giá trị tự động cho bộ lọc ComboBox
                            SelectedMonth = firstPending.Month;
                            SelectedYear = firstPending.Year;
                            SelectedDepartment = targetDept; // Khi gán cái này, OnSelectedDepartmentChanged sẽ tự kích hoạt gọi LoadPendingPayrolls() bên dưới luôn!

                            return; // Tìm thấy rồi thì thoát hàm, để hệ thống tự load dữ liệu lên
                        }
                    }

                    // Fallback: Nếu không có bảng nào Pending cả, gán mặc định về tháng/năm hiện tại để tránh bị trống bộ lọc
                    if (SelectedMonth == 0) SelectedMonth = DateTime.Now.Month;
                    if (SelectedYear == 0) SelectedYear = DateTime.Now.Year;
                    if (SelectedDepartment == null && Departments.Any()) SelectedDepartment = Departments.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AutoDetect Error: " + ex.Message);
            }
        }

        /// <summary>
        /// ✅ 1. Logic PHÊ DUYỆT TOÀN BỘ BẢNG LƯƠNG của phòng ban đang chọn
        /// </summary>
        [RelayCommand]
        private void ApprovePayrollSheet()
        {
            if (_currentTargetStatus == null || !PendingPayrolls.Any())
            {
                MessageBox.Show("There are no pending payroll submissions available for this selection to approve!",
                                "Approval Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string deptName = SelectedDepartment?.DepartmentName ?? "Selected Department";
            var confirmResult = MessageBox.Show($"Are you sure you want to APPROVE the entire payroll sheet for department '{deptName}' ({SelectedMonth}/{SelectedYear})?\nThis action updates status to Approved, locks records, and unlocks data for Dashboard Charts.",
                                                "Sheet Approval Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.OK) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var record = db.DepartmentPayrollStatuses.Find(_currentTargetStatus.PayrollStatusID);
                    if (record != null)
                    {
                        record.Status = "Approved";
                        record.ApprovedBy = UserSession.Instance.UserID;
                        record.ApprovalDate = DateTime.Now;

                        if (db.SaveChanges() > 0)
                        {
                            UserRepository.LogAction(
                                UserSession.Instance.UserID,
                                "APPROVE_PAYROLL_SHEET",
                                $"Approved payroll sheet for department '{deptName}' (Period: {SelectedMonth}/{SelectedYear}). Total records locked."
                            );

                            MessageBox.Show($"Payroll sheet for department '{deptName}' has been successfully APPROVED and locked!\nNotification dispatched to HR Staff.",
                                            "Approval Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadPendingPayrolls(); // Làm mới giao diện sạch sẽ
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing sheet approval: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 🚫 2. Luồng TỪ CHỐI TOÀN BỘ BẢNG LƯƠNG phòng ban kèm Feedback lý do
        /// </summary>
        [RelayCommand]
        private void OpenRejectPopup()
        {
            if (_currentTargetStatus == null || !PendingPayrolls.Any())
            {
                MessageBox.Show("There are no pending payroll submissions available for this selection to decline!",
                                "Action Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gợi ý sẵn một câu phản hồi chuẩn để sếp đỡ mất công gõ nhiều
            RejectReasonInput = "Incorrect basic salary, bonus, or deduction calculations. Please re-verify with validated financial logs.";
            IsRejectPopupOpen = true;
        }

        [RelayCommand]
        private void CloseRejectPopup()
        {
            IsRejectPopupOpen = false;
        }

        // Thực thi bấm nút xác nhận từ chối gửi lệnh xuống DB
        [RelayCommand]
        private void ConfirmRejectPayroll()
        {
            if (_currentTargetStatus == null) return;

            if (string.IsNullOrWhiteSpace(RejectReasonInput))
            {
                MessageBox.Show("Please provide a rejection reason/feedback to the HR Staff before submitting!",
                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string deptName = SelectedDepartment?.DepartmentName ?? "Selected Department";

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var record = db.DepartmentPayrollStatuses.Find(_currentTargetStatus.PayrollStatusID);
                    if (record != null)
                    {
                        record.Status = "Rejected";
                        record.RejectReason = RejectReasonInput.Trim();
                        record.ApprovedBy = UserSession.Instance.UserID;
                        record.ApprovalDate = DateTime.Now;

                        if (db.SaveChanges() > 0)
                        {
                            UserRepository.LogAction(
                                UserSession.Instance.UserID,
                                "REJECT_PAYROLL",
                                $"Rejected payroll sheet for department '{deptName}' (Period: {SelectedMonth}/{SelectedYear}). Reason: {record.RejectReason}"
                            );

                            MessageBox.Show($"Payroll sheet for '{deptName}' has been marked as [Declined]. Feedback has been successfully dispatched to HR Staff for corrections.",
                                            "Feedback Dispatched", MessageBoxButton.OK, MessageBoxImage.Information);
                            //Dọn sạch lưới dữ liệu và xóa vết trạng thái cũ ngay khi từ chối thành công
                            PendingPayrolls.Clear();
                            _currentTargetStatus = null;
                            IsRejectPopupOpen = false;
                            LoadPendingPayrolls(); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing rejection: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}