using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class TransferHistory
    {
        public int TransferID { get; set; }
        public int? EmployeeID { get; set; }
        public int? FromDepartmentID { get; set; }
        public int? ToDepartmentID { get; set; }
        public DateTime TransferDate { get; set; }
    }
}
