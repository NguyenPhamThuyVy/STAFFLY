using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StafflyApp.Data.Interfaces;
using StafflyApp.Models;

namespace StafflyApp.Data.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        // 1. Hàm lấy danh sách tất cả nhân viên
        public List<Employee> GetAllEmployees()
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                string query = "SELECT EmployeeID, FullName, Email, Phone, Address, DateOfBirth, DepartmentID, Status FROM Employees";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Employee emp = new Employee
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                                DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]) : (DateTime?)null,
                                DepartmentID = reader["DepartmentID"] != DBNull.Value ? Convert.ToInt32(reader["DepartmentID"]) : (int?)null,
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null
                            };
                            employees.Add(emp);
                        }
                    }
                }
            }
            return employees;
        }

        // 2. Thêm nhân viên mới
        public bool AddEmployee(Employee emp)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"INSERT INTO Employees (FullName, Email, Phone, Address, DateOfBirth, DepartmentID, Status) 
                                VALUES (@Name, @Email, @Phone, @Address, @DOB, @DeptID, @Status)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", emp.FullName);
                cmd.Parameters.AddWithValue("@Email", (object)emp.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)emp.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)emp.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DOB", (object)emp.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DeptID", (object)emp.DepartmentID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 3. Cập nhật thông tin
        public bool UpdateEmployee(Employee emp)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"UPDATE Employees 
                                SET FullName=@Name, Email=@Email, Phone=@Phone, Address=@Address, 
                                    DateOfBirth=@DOB, DepartmentID=@DeptID, Status=@Status 
                                WHERE EmployeeID=@ID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", emp.EmployeeID);
                cmd.Parameters.AddWithValue("@Name", emp.FullName);
                cmd.Parameters.AddWithValue("@Email", (object)emp.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)emp.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)emp.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DOB", (object)emp.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DeptID", (object)emp.DepartmentID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 4. Xóa nhân viên
        public bool DeleteEmployee(int id)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = "DELETE FROM Employees WHERE EmployeeID = @ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", id);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}