using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }
        public string Status { get; set; } = "Active"; // Mặc định là Active
        [NotMapped]
        public string DepartmentName { get; set; }
        [NotMapped]
        public string ContractType { get; set; }
        public string? Position { get; set; }
        public DateTime? StartDate { get; set; }
    }
}

