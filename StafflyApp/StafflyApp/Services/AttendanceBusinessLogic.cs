using StafflyApp.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Services
{
    public class AttendanceBusinessLogic
    {
        /// <summary>
        /// Hàm tổng hợp dữ liệu chấm công chi tiết thành bảng tổng hợp tháng
        /// </summary>
        /// <param name="dailyAttendances">Danh sách công chi tiết của các nhân viên trong tháng cần tính</param>
        /// <param name="month">Tháng tính công</param>
        /// <param name="year">Năm tính công</param>
        public List<AttendanceSummaryDto> CalculateMonthlySummary(List<DailyAttendanceDto> dailyAttendances, int month, int year)
        {
            if (dailyAttendances == null || !dailyAttendances.Any())
                return new List<AttendanceSummaryDto>();

            var summaryList = dailyAttendances
                .GroupBy(d => d.EmployeeId)
                .Select(group => new AttendanceSummaryDto
                {
                    EmployeeId = group.Key,
                    Month = month,
                    Year = year,

                    // 1. Tổng ngày công làm việc thực tế (Quét vân tay có đi làm)
                    TotalStandardWorkDays = group.Sum(d => d.WorkAttendance),

                    // 2. Tổng ngày nghỉ phép hưởng lương (Phép năm, chế độ...)
                    TotalPaidLeaveDays = group.Sum(d => d.PaidLeaveAttendance),

                    // 3. Tổng ngày tính lương thực tế = Công đi làm + Công ngày phép
                    TotalSalaryDays = group.Sum(d => d.WorkAttendance + d.PaidLeaveAttendance),

                    // 4. Tổng số ngày nghỉ không hưởng lương
                    TotalUnpaidLeaveDays = group.Sum(d => d.UnpaidLeaveDays),

                    // 5. Tổng số giờ tăng ca (OT)
                    TotalOvertimeHours = group.Sum(d => d.OvertimeHours)
                })
                .ToList();

            return summaryList;
        }
    }
}
