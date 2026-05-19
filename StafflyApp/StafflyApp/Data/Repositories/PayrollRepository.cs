using Microsoft.EntityFrameworkCore;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
namespace StafflyApp.Data.Repositories
{
    public class PayrollRepository
    {
        private readonly StafflyDbContext _context;

        // Constructor dùng chung cho các hàm Async sử dụng DI _context
        public PayrollRepository(StafflyDbContext context)
        {
            _context = context;
        }

        // Hoặc tạo thêm Constructor rỗng phòng trường hợp các chỗ khác trong code gọi New không truyền tham số
        public PayrollRepository()
        {
            _context = new StafflyDbContext();
        }

        // 1. Hàm cập nhật trạng thái phê duyệt/từ chối bảng lương 
        public bool UpdatePayrollStatus(int payrollId, string status, int approvedById)
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var payroll = db.Payrolls.Find(payrollId);
                    if (payroll == null) return false;

                    payroll.Status = status;
                    payroll.ApprovedBy = approvedById;
                    payroll.UpdatedAt = DateTime.Now;

                    if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        LockPayrollData(db, payrollId);
                    }

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
                var attendances = db.Attendances
                                    .Where(a => a.EmployeeID == payroll.EmployeeID
                                             && a.Date.HasValue
                                             && a.Date.Value.Month == payroll.Month
                                             && a.Date.Value.Year == payroll.Year)
                                    .ToList();

                foreach (var attendance in attendances)
                {
                    attendance.IsLocked = true;
                }
            }
        }

        // 3. Hàm kiểm tra xem một chu kỳ lương đã bị khóa (Approved) chưa
        public bool IsPayrollLocked(int employeeId, int month, int year)
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
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

        // 4. Hàm lưu danh sách bảng lương sau khi Staff Import Excel thành công
        public async Task<bool> SavePayrollRangeAsync(List<Payroll> payrolls)
        {
            try
            {
                foreach (var payroll in payrolls)
                {
                    _context.Entry(payroll).State = EntityState.Added;
                }

                int result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DbSaveError: {ex.Message}");
                return false;
            }
        }

        // 5. Đồng bộ check chu kỳ theo Month/Year thật của Model 
        public async Task<bool> IsPayrollPeriodExistedAsync(int employeeId, int month, int year)
        {
            return await _context.Payrolls.AnyAsync(p =>
                p.EmployeeID == employeeId &&
                p.Month == month &&
                p.Year == year);
        }

        // 6. Đồng bộ cột ngày chấm công thành a.Date cho khớp Model Attendance
        public async Task<bool> CheckAttendanceLockStatusAsync(int employeeId, DateTime? date)
        {
            if (date == null) return false;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeID == employeeId && a.Date == date);

            if (attendance == null) return false;

            // Kiểm tra cờ IsLocked của ngày công
            return attendance.IsLocked;
        }
    }
}