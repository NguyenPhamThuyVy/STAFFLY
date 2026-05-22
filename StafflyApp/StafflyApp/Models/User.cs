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
        public string? RoleName { get; set; } 
        public int? EmployeeID { get; set; } 
        public bool IsActive { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsDefaultPassword { get; set; } // Cờ Bit kiểm tra mật khẩu mặc định (True/False)
        public DateTime CreatedAt { get; set; }
        public bool IsResetRequested { get; set; } // Cờ báo xem User có đang đòi reset pass không
        public string? TempPasswordPlain { get; set; } // Lưu pass thô tạm thời để tự điền ở màn Login cho user
    }
}
