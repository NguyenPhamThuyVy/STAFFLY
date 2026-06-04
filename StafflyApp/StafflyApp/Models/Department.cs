using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int HeadcountLimit { get; set; }
        public int CurrentStaffCount { get; set; }
        // Giúp Entity Framework hiểu một phòng ban có nhiều nhân viên
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
