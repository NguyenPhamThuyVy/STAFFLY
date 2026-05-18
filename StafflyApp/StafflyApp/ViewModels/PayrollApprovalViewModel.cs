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

        // Danh sách bảng lương Pending 
        [ObservableProperty] private ObservableCollection<Payroll> _pendingPayrolls = new();

        // Thuộc tính nhận lý do từ chối
        [ObservableProperty] private string _rejectReasonInput = string.Empty;

        public PayrollApprovalViewModel()
        {
            LoadPendingData();
        }

        // 1. Hàm bốc dữ liệu Pending từ DB lên nuôi hệ thống
        public void LoadPendingData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Lấy các bản ghi có Status là Pending
                    var list = db.Payrolls.Where(p => p.Status == "Pending").ToList();

                    // Kết hợp bốc tên nhân viên từ bảng Employees lên để hiển thị
                    foreach (var p in list)
                    {
                        var emp = db.Employees.Find(p.EmployeeID);
                        p.EmployeeName = emp?.FullName ?? "Unknown Employee";
                    }

                    PendingPayrolls = new ObservableCollection<Payroll>(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadPendingData Error: " + ex.Message);
            }
        }

        // 2. Logic xử lý khi Manager ấn nút ACCEPT (Phê duyệt)
        [RelayCommand]
        private void AcceptPayroll(Payroll payroll)
        {
            if (payroll == null) return;

            // Gọi hàm Repo đổi trạng thái thành Approved và tự động khóa dữ liệu chấm công tháng đó lại
            bool isSuccess = _payrollRepo.UpdatePayrollStatus(payroll.PayrollID, "Approved", UserSession.Instance.UserID);

            if (isSuccess)
            {
                MessageBox.Show($"Payroll for Employee ID {payroll.EmployeeID} has been APPROVED and attendance data is locked!",
                                "Approval Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPendingData(); // Refresh lại danh sách
            }
        }

        // 3. Logic xử lý khi Manager ấn nút DECLINE (Từ chối)
        [RelayCommand]
        private void DeclinePayroll(Payroll payroll)
        {
            if (payroll == null) return;

            // BƯỚC ĐỆM KHI CHƯA CÓ UI: Dùng Microsoft.VisualBasic Interaction.InputBox 
            // để test nhanh luồng nhập dữ liệu mà không cần vẽ màn hình popup phức tạp!
            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                $"Enter the reason for declining Employee ID {payroll.EmployeeID}'s payroll:",
                "Decline Reason Required",
                "Incorrect bonus configuration."
            );

            // Nếu sếp bấm Cancel hoặc bỏ trống thì chặn không cho từ chối lụi
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("You must provide a reason to decline this payroll submission!",
                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tiến hành cập nhật lý do và trạng thái Rejected xuống SQL Server
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var record = db.Payrolls.Find(payroll.PayrollID);
                    if (record != null)
                    {
                        record.Status = "Rejected";
                        record.RejectReason = reason; // Lưu vết chuỗi lý do sếp vừa gõ
                        record.ApprovedBy = UserSession.Instance.UserID;
                        record.UpdatedAt = DateTime.Now;

                        if (db.SaveChanges() > 0)
                        {
                            MessageBox.Show("Payroll declined successfully. Feedback has been sent back to HR Staff.",
                                            "Declined", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadPendingData(); // Tải lại lưới dữ liệu
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error declining payroll: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}