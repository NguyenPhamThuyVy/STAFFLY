using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? DepartmentID { get; set; }
        public string Status { get; set; }
    }
}
