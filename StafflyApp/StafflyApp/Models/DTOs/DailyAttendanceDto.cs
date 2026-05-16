using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models.DTOs
{
    public class DailyAttendanceDto
    {
        public string EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public double WorkAttendance { get; set; } // Số công làm việc thực tế (e.g., 1.0, 0.5, 0)
        public double PaidLeaveAttendance { get; set; } // Số công được tính do nghỉ phép hưởng lương (AL)
        public double UnpaidLeaveDays { get; set; } // Số ngày nghỉ không lương (UL)
        public double OvertimeHours { get; set; } // Số giờ làm thêm (OT)
    }
}
