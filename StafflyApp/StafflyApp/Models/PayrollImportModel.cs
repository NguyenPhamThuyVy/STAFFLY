using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class PayrollImportModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal TotalBonus { get; set; }
        public bool IsValid { get; set; }
        public string ErrorNote { get; set; }
    }
}
