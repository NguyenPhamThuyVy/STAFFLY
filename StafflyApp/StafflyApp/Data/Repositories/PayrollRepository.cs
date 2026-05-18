using System;
using System.Collections.Generic;
using System.Linq;
using StafflyApp.Data;
using StafflyApp.Models;

namespace StafflyApp.Data.Repositories
{
    public class PayrollRepository
    {
        // 1. Hàm cập nhật trạng thái phê duyệt/từ chối bảng lương 
        public bool UpdatePayrollStatus(int payrollId, string status, int approvedById)
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Tìm bản ghi bảng lương theo ID
                    var payroll = db.Payrolls.Find(payrollId);
                    if (payroll == null) return false;

                    // Cập nhật các thông tin phê duyệt
                    payroll.Status = status; // "Approved" hoặc "Rejected"

                    // SỬA LỖI 1: Đổi tham số truyền vào thành int approvedById để khớp kiểu int? của Model
                    payroll.ApprovedBy = approvedById;

                    // SỬA LỖI 2: Dòng này sẽ hết lỗi sau khi Vy thêm thuộc tính UpdatedAt vào file Payroll.cs
                    payroll.UpdatedAt = DateTime.Now;

                    // Nếu được duyệt (Approved) thì tự động khóa dữ liệu chu kỳ đó lại
                    if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        LockPayrollData(db, payrollId);
                    }

                    // Lưu thay đổi xuống SQL Server
                    return db.SaveChanges() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // 2. Hàm nội bộ dùng để thực hiện khóa dữ liệu ngày công 
        private void LockPayrollData(StafflyDbContext db, int payrollId)
        {
            var payroll = db.Payrolls.Find(payrollId);
            if (payroll != null)
            {
                // Tìm tất cả các bản ghi chấm công (Attendance) của nhân viên này trong tháng/năm đó
                // SỬA LỖI 3 & 4: Dùng .Value.Month và .Value.Year để bóc tách dữ liệu từ kiểu DateTime? (Nullable)
                var attendances = db.Attendances
                                    .Where(a => a.EmployeeID == payroll.EmployeeID
                                             && a.Date.HasValue
                                             && a.Date.Value.Month == payroll.Month
                                             && a.Date.Value.Year == payroll.Year)
                                    .ToList();

                // Đổi trạng thái toàn bộ ngày công thành Locked (HR Staff sẽ không sửa được nữa)
                foreach (var attendance in attendances)
                {
                    attendance.IsLocked = true;
                }
            }
        }

        // 3. Hàm kiểm tra xem một chu kỳ lương đã bị khóa (Approved) chưa (Phục vụ cụm chặn Guard Clause)
        public bool IsPayrollLocked(int employeeId, int month, int year)
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Nếu bảng lương của nhân viên này ở tháng/năm này đã "Approved", trả về true (Đã khóa)
                    return db.Payrolls.Any(p => p.EmployeeID == employeeId
                                             && p.Month == month
                                             && p.Year == year
                                             && p.Status == "Approved");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}