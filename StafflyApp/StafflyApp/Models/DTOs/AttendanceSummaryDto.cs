using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models.DTOs
{
    public class AttendanceSummaryDto
    {
        public string EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // Các chỉ số phục vụ tính lương
        public double TotalStandardWorkDays { get; set; } // Tổng ngày công làm việc thực tế
        public double TotalPaidLeaveDays { get; set; }    // Tổng ngày nghỉ phép vẫn tính lương
        public double TotalSalaryDays { get; set; }       // Tổng ngày quy đổi tính lương (= Thực tế + Phép)
        public double TotalUnpaidLeaveDays { get; set; }  // Tổng ngày nghỉ không lương (để trừ lương đóng bảo hiểm nếu cần)
        public double TotalOvertimeHours { get; set; }     // Tổng giờ OT trong tháng
    }
}
