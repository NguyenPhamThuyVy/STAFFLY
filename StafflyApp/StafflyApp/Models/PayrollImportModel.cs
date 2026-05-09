using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class PayrollImportModel
    {
        public string EmployeeID { get; set; }
        public string FullName { get; set; }
        public decimal BasicSalary { get; set; }
        public int WorkingDays { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
    }
}
