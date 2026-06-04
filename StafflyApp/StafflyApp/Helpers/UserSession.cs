using System;

namespace StafflyApp.Helpers
{
    // Áp dụng Design Pattern Singleton để duy nhất 1 phiên đăng nhập tồn tại trong suốt vòng đời App
    public class UserSession
    {
        private static UserSession _instance;
        public static UserSession Instance => _instance ??= new UserSession();

        public int UserID { get; set; }
        public string Username { get; set; }
        public int RoleID { get; set; } // 1: Admin, 2: Manager, 3: Staff
        public string RoleName { get; set; }

        private UserSession() { }

        // Hàm gọi khi đăng xuất
        public void ClearSession()
        {
            UserID = 0;
            Username = string.Empty;
            RoleID = 0;
            RoleName = string.Empty;
        }
    }
}