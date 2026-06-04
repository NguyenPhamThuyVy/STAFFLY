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

        public PayrollRepository(StafflyDbContext context)
        {
            _context = context;
        }
        public PayrollRepository()
        {
            _context = new StafflyDbContext();
        }

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
        private void LockPayrollData(StafflyDbContext db, int payrollId)
        {
            var payroll = db.Payrolls.Find(payrollId);
            if (payroll != null)
            {
                var attendances = db.Attendances
                                    .Where(a => a.EmployeeID == payroll.EmployeeID
                                             && a.Date.Month == payroll.Month
                                             && a.Date.Year == payroll.Year)
                                    .ToList();

                foreach (var attendance in attendances)
                {
                    attendance.IsLocked = true;
                }
            }
        }

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

        public async Task<bool> IsPayrollPeriodExistedAsync(int employeeId, int month, int year)
        {
            return await _context.Payrolls.AnyAsync(p =>
                p.EmployeeID == employeeId &&
                p.Month == month &&
                p.Year == year);
        }

        public async Task<bool> CheckAttendanceLockStatusAsync(int employeeId, DateTime? date)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeID == employeeId && a.Date == date);

            if (attendance == null) return false;

            return attendance.IsLocked;
        }
    }
}