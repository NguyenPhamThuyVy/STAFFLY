using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace StafflyApp.Helpers
{
    public static class PasswordHelper
    {
        // 1. Hàm dùng khi tạo tài khoản mới (Hash mật khẩu)
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // 2. Hàm dùng khi đăng nhập (Kiểm tra mật khẩu khớp không)
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}