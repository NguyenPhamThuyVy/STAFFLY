using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class AuditLog
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string Action { get; set; }
        public string Detail { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
