using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Helpers;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace StafflyApp.ViewModels
{
    public partial class PayrollApprovalViewModel : ObservableObject
    {
        // Sử dụng danh sách các phòng ban đang chờ duyệt để hiển thị lên lưới của Manager
        [ObservableProperty] private ObservableCollection<DepartmentPayrollStatus> _pendingDepartments = new();
        [ObservableProperty] private string _rejectReasonInput = string.Empty;

        // Quản lý trạng thái Popup ẩn/hiện điền lý do từ chối
        [ObservableProperty] private bool _isDeclinePopupOpen = false;

        // Lưu vết phòng ban đang được chọn để xử lý từ chối
        [ObservableProperty] private DepartmentPayrollStatus? _selectedDepartmentStatus;

        public PayrollApprovalViewModel()
        {
            // Tải danh sách các phòng ban đang gửi bảng lương chờ duyệt ngay khi mở màn hình
            LoadPendingData();
        }

        public void LoadPendingData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Lấy các bản ghi trạng thái là 'Pending', đồng thời nạp kèm (Include) thông tin tên Phòng ban
                    var list = db.DepartmentPayrollStatuses
                                 .Include(s => s.Department)
                                 .Where(s => s.Status == "Pending")
                                 .ToList();

                    // Đổ dữ liệu lên giao diện
                    PendingDepartments = new ObservableCollection<DepartmentPayrollStatus>(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadPendingData Error: " + ex.Message);
            }
        }

        // 1. Logic phê duyệt từng phòng ban được chọn 
        [RelayCommand]
        private void ApproveDepartmentPayroll(DepartmentPayrollStatus deptStatus)
        {
            if (deptStatus == null) return;

            string deptName = deptStatus.Department?.DepartmentName ?? "Unknown";
            var confirmResult = MessageBox.Show($"Are you sure you want to APPROVE the payroll for department '{deptName}' (Period: {deptStatus.Month}/{deptStatus.Year})?\nThis action will lock financial records and unlock data for Charts.",
                                                "Approval Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.OK) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var record = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.PayrollStatusID == deptStatus.PayrollStatusID);

                    if (record != null)
                    {
                        record.Status = "Approved";
                        record.ApprovedBy = UserSession.Instance.UserID;
                        record.ApprovalDate = DateTime.Now;

                        if (db.SaveChanges() > 0)
                        {
                            // Ghi log 
                            UserRepository.LogAction(
                                UserSession.Instance.UserID,
                                "APPROVE_PAYROLL",
                                $"Approved payroll sheet for department '{deptName}' (Period: {record.Month}/{record.Year})."
                            );

                            MessageBox.Show($"Payroll for department '{deptName}' has been successfully APPROVED and locked!",
                                            "Approval Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadPendingData(); // Làm mới danh sách hiển thị
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during approval processing: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. Logic từ chối lương phòng ban 
        [RelayCommand]
        private void OpenDeclinePopup(DepartmentPayrollStatus deptStatus)
        {
            if (deptStatus == null) return;
            SelectedDepartmentStatus = deptStatus;
            RejectReasonInput = "Incorrect basic salary, bonus, or deduction calculations."; 
            IsDeclinePopupOpen = true;
        }

        [RelayCommand]
        private void CloseDeclinePopup()
        {
            IsDeclinePopupOpen = false;
            SelectedDepartmentStatus = null;
        }

        // Thực thi gửi lệnh từ chối phòng ban xuống DB
        [RelayCommand]
        private void ConfirmDeclinePayroll()
        {
            if (SelectedDepartmentStatus == null) return;

            if (string.IsNullOrWhiteSpace(RejectReasonInput))
            {
                MessageBox.Show("Please specify the reason for declining this department's payroll submission!",
                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string deptName = SelectedDepartmentStatus.Department?.DepartmentName ?? "Unknown";

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var record = db.DepartmentPayrollStatuses.Find(SelectedDepartmentStatus.PayrollStatusID);
                    if (record != null)
                    {
                        record.Status = "Rejected";
                        record.RejectReason = RejectReasonInput.Trim();
                        record.ApprovedBy = UserSession.Instance.UserID;
                        record.ApprovalDate = DateTime.Now; 

                        if (db.SaveChanges() > 0)
                        {
                            // Ghi log chi tiết lý do từ chối 
                            UserRepository.LogAction(
                                UserSession.Instance.UserID,
                                "REJECT_PAYROLL",
                                $"Rejected payroll for department '{deptName}' (Period: {record.Month}/{record.Year}). Reason: {record.RejectReason}"
                            );

                            MessageBox.Show($"Payroll submission for '{deptName}' has been marked as [Declined]. Notification sent back to HR Accountant.",
                                            "Feedback Dispatched", MessageBoxButton.OK, MessageBoxImage.Information);

                            IsDeclinePopupOpen = false;
                            LoadPendingData(); 
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