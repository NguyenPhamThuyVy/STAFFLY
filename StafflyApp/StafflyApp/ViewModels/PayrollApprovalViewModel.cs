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

namespace StafflyApp.ViewModels
{
    public partial class PayrollApprovalViewModel : ObservableObject
    {
        private readonly PayrollRepository _payrollRepo = new();

        [ObservableProperty] private ObservableCollection<Payroll> _pendingPayrolls = new();
        [ObservableProperty] private string _rejectReasonInput = string.Empty;

        // Quản lý trạng thái Popup ẩn/hiện
        [ObservableProperty] private bool _isDeclinePopupOpen = false;

        // Lưu vết dòng bản ghi lương đang được chọn để từ chối
        [ObservableProperty] private Payroll? _selectedPayroll;

        public PayrollApprovalViewModel()
        {
            // Đảm bảo bốc dữ liệu Pending ngay khi Manager vừa load View
            LoadPendingData();
        }

        public void LoadPendingData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Tải danh sách Pending
                    var list = db.Payrolls.Where(p => p.Status == "Pending").ToList();

                    foreach (var p in list)
                    {
                        var emp = db.Employees.Find(p.EmployeeID);
                        p.EmployeeName = emp?.FullName ?? "Unknown Employee";
                    }

                    // Gán đè ObservableCollection mới để kích hoạt giao diện DataGrid update
                    PendingPayrolls = new ObservableCollection<Payroll>(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadPendingData Error: " + ex.Message);
            }
        }

        [RelayCommand]
        private void AcceptPayroll(Payroll payroll)
        {
            if (payroll == null) return;

            bool isSuccess = _payrollRepo.UpdatePayrollStatus(payroll.PayrollID, "Approved", UserSession.Instance.UserID);

            if (isSuccess)
            {
                MessageBox.Show($"Payroll for {payroll.EmployeeName} has been APPROVED and attendance data is locked!",
                                "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPendingData();
            }
        }

        // THÊM VÀO LUỒNG NGHIỆP VỤ BATCH UPDATE: Logic xử lý phê duyệt đồng loạt
        [RelayCommand]
        private void AcceptAllPayrolls()
        {
            if (PendingPayrolls == null || !PendingPayrolls.Any())
            {
                MessageBox.Show("There are no pending payroll records to approve!",
                                "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to APPROVE ALL {PendingPayrolls.Count} pending payroll records at once?\nThis action will lock attendance logs for this period.",
                                                "Bulk Approval Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.OK) return;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Thu thập tập hợp khóa chính ID hiện diện trên lưới UI để lọc
                    var pendingIds = PendingPayrolls.Select(p => p.PayrollID).ToList();
                    var recordsInDb = db.Payrolls.Where(p => pendingIds.Contains(p.PayrollID)).ToList();

                    if (recordsInDb.Any())
                    {
                        foreach (var record in recordsInDb)
                        {
                            record.Status = "Approved";
                            record.ApprovedBy = UserSession.Instance.UserID;
                            record.UpdatedAt = DateTime.Now;
                        }

                        // Thực thi lưu đồng loạt trong một phiên giao dịch duy nhất xuống SQL Server
                        if (db.SaveChanges() > 0)
                        {
                            MessageBox.Show($"All {recordsInDb.Count} payroll records have been successfully APPROVED and finalized!",
                                            "Bulk Approval Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadPendingData(); // Làm sạch lưới dữ liệu hiển thị
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during bulk approval processing: " + ex.Message,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Kích hoạt mở Popup điền Feedback
        [RelayCommand]
        private void OpenDeclinePopup(Payroll payroll)
        {
            if (payroll == null) return;
            SelectedPayroll = payroll;
            RejectReasonInput = "Incorrect bonus/allowance calculation."; // Đặt chữ gợi ý sẵn
            IsDeclinePopupOpen = true;
        }

        [RelayCommand]
        private void CloseDeclinePopup()
        {
            IsDeclinePopupOpen = false;
            SelectedPayroll = null;
        }

        // Thực thi gửi tín hiệu Từ chối dữ liệu xuống DB
        [RelayCommand]
        private void ConfirmDeclinePayroll()
        {
            if (SelectedPayroll == null) return;

            if (string.IsNullOrWhiteSpace(RejectReasonInput))
            {
                MessageBox.Show("Please specify the reason for declining this payroll submission!",
                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var record = db.Payrolls.Find(SelectedPayroll.PayrollID);
                    if (record != null)
                    {
                        record.Status = "Rejected";
                        record.RejectReason = RejectReasonInput.Trim();
                        record.ApprovedBy = UserSession.Instance.UserID;
                        record.UpdatedAt = DateTime.Now;

                        if (db.SaveChanges() > 0)
                        {
                            MessageBox.Show($"Payroll submission marked as [Declined]. Reason dispatched back to HR Staff.",
                                            "Feedback Dispatched", MessageBoxButton.OK, MessageBoxImage.Information);

                            IsDeclinePopupOpen = false; // Đóng popup thành công
                            LoadPendingData(); // Refresh lưới dữ liệu sạch
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