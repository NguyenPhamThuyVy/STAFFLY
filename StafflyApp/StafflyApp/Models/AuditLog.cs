using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace StafflyApp.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string Action { get; set; }
        public string Detail { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
