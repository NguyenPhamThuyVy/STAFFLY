using Microsoft.Data.SqlClient;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.Collections.Generic;

public class EmployeeRepository
{
    // 1. Lấy danh sách (Soft Delete)
    public List<Employee> GetAllEmployees()
    {
        List<Employee> employees = new List<Employee>();
        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
        {
            conn.Open();
            string query = "SELECT * FROM Employees WHERE Status != 'Resigned' OR Status IS NULL";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(MapReaderToEmployee(reader));
                    }
                }
            }
        }
        return employees;
    }

    // 2. THÊM MỚI (Hàm này đang thiếu trong hình của bạn nè)
    public bool AddEmployee(Employee emp)
    {
        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
        {
            string query = @"INSERT INTO Employees (FullName, Email, DateOfBirth, Status) 
                            VALUES (@Name, @Email, @DOB, @Status)";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", emp.FullName);
            cmd.Parameters.AddWithValue("@Email", (object)emp.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DOB", (object)emp.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // 3. CẬP NHẬT
    public bool UpdateEmployee(Employee emp)
    {
        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
        {
            string query = @"UPDATE Employees SET FullName=@Name, Email=@Email, 
                            DateOfBirth=@DOB, Status=@Status WHERE EmployeeID=@ID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", emp.EmployeeID);
            cmd.Parameters.AddWithValue("@Name", emp.FullName);
            cmd.Parameters.AddWithValue("@Email", (object)emp.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DOB", (object)emp.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // 4. XÓA MỀM (Soft Delete)
    public bool DeleteEmployee(int id)
    {
        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
        {
            string query = "UPDATE Employees SET Status = 'Resigned' WHERE EmployeeID = @ID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", id);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private Employee MapReaderToEmployee(SqlDataReader reader)
    {
        return new Employee
        {
            EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
            FullName = reader["FullName"].ToString(),
            Email = reader["Email"]?.ToString(),
            DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]) : (DateTime?)null,
            Status = reader["Status"]?.ToString()
        };
    }
}