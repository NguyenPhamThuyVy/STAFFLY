using Microsoft.Data.SqlClient;
using StafflyApp.Data.Interfaces;
using StafflyApp.Helpers;
using StafflyApp.Models;
using System.Linq;

namespace StafflyApp.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly StafflyDbContext _context;

        public UserRepository(StafflyDbContext context)
        {
            _context = context;
        }

        public User? AuthenticateUser(string username, string password)
        {
            using (var conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                // Chỉ tìm theo Username 
                string query = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hashedPasswordInDb = reader["Password"].ToString();

                        // Dùng Helper để so khớp mật khẩu
                        if (PasswordHelper.VerifyPassword(password, hashedPasswordInDb))
                        {
                            return new User
                            {
                                UserID = (int)reader["UserID"],
                                Username = reader["Username"].ToString(),
                                RoleID = reader["RoleID"] as int?
                                // ... map thêm các trường khác nếu cần
                            };
                        }
                    }
                }
            }
            return null; // Sai pass hoặc không tìm thấy user
        }

        public Employee? GetEmployeeByUserId(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null && user.EmployeeID.HasValue)
            {
                return _context.Employees.Find(user.EmployeeID.Value);
            }
            return null;
        }
    }
}