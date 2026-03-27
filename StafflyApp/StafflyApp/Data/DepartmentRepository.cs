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
    }
}