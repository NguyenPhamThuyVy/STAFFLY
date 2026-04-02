using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StafflyApp.Models;

namespace StafflyApp.Data
{
    public class DepartmentRepository
    {
        public List<Department> GetAllDepartments()
        {
            List<Department> departments = new List<Department>();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                // Truy vấn đúng các cột bạn có trong Model
                string query = "SELECT DepartmentID, DepartmentName, HeadcountLimit, CurrentStaffCount FROM Departments";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new Department
                            {
                                DepartmentID = Convert.ToInt32(reader["DepartmentID"]),
                                DepartmentName = reader["DepartmentName"].ToString() ?? "",
                                HeadcountLimit = Convert.ToInt32(reader["HeadcountLimit"]),
                                CurrentStaffCount = Convert.ToInt32(reader["CurrentStaffCount"])
                            });
                        }
                    }
                }
            }
            return departments;
        }
        public bool IsSpaceAvailable(int departmentId)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = "SELECT (HeadcountLimit - CurrentStaffCount) FROM Departments WHERE DepartmentID = @DeptID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DeptID", departmentId);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        int remainingSlots = Convert.ToInt32(result);
                        return remainingSlots > 0; 
                    }
                }
            }
            return false; 
        }

        // Hàm trả về số lượng chỗ trống cụ thể
        public int GetRemainingSlots(int departmentId)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = "SELECT (HeadcountLimit - CurrentStaffCount) FROM Departments WHERE DepartmentID = @DeptID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DeptID", departmentId);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }
    }
}