using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class Payroll
    {
        public int PayrollID { get; set; }
        public int? EmployeeID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalSalary { get; set; }
        public string Status { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
