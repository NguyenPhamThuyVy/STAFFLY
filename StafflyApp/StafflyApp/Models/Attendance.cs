using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class Attendance
    {
        public int AttendanceID { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
    }
}
