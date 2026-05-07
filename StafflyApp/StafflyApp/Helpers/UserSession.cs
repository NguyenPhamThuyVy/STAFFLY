using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Helpers
{
    public static class UserSession
    {
        // ID của người dùng (để truy vấn dữ liệu liên quan)
        public static int UserID { get; set; }

        // Tên hiển thị trên Dashboard 
        public static string? Username { get; set; }

        // Vai trò để phân quyền (Admin, HR_Manager, HR_Staff)
        public static int RoleID { get; set; }
        public static string? RoleName { get; set; }

        // Hàm để xóa dữ liệu khi Logout
        public static void Logout()
        {
            UserID = 0;
            Username = null;
            RoleID = 0;
            RoleName = null;
        }
    }
}
