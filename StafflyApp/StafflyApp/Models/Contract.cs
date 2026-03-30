using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class Contract
    {
        public int ContractID { get; set; }
        public int? EmployeeID { get; set; }
        public string ContractType { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal BasicSalary { get; set; }
    }
}
