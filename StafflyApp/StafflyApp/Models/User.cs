using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? RoleID { get; set; }
        public int? EmployeeID { get; set; } 
        public bool IsActive { get; set; } 
    }
}
