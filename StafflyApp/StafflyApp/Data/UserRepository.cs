using System;
using Microsoft.Data.SqlClient;
using StafflyApp.Models;

namespace StafflyApp.Data
{
    public class UserRepository
    {
        public User? AuthenticateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                // Truy vấn tìm User khớp cả tên và mật khẩu
                string query = "SELECT * FROM Users WHERE Username = @user AND Password = @pass";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                Username = reader["Username"].ToString() ?? string.Empty,
                                RoleID = reader["RoleID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RoleID"])
                            };
                        }
                    }
                }
            }
            return null; // Trả về null nếu không tìm thấy (sai tài khoản/mật khẩu)
        }
    }
}