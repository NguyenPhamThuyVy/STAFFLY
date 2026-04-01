using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using StafflyApp.Data;
using System.ComponentModel;
using System.IO;

public class ExcelService
{
    private readonly EmployeeRepository _repo = new EmployeeRepository();

    public List<string> CheckPayrollExcel(string filePath)
    {
        var reports = new List<string>();
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) 
            {
                var cellValue = worksheet.Cells[row, 1].Value; // Giả sử cột 1 là ID
                if (cellValue != null && int.TryParse(cellValue.ToString(), out int empId))
                {
                    // Kiểm tra ID có tồn tại trong DB không
                    if (!CheckIdExists(empId))
                    {
                        reports.Add($"Dòng {row}: Cảnh báo - EmployeeID {empId} không tồn tại!");
                    }
                }
            }
        }
        return reports;
    }

    private bool CheckIdExists(int id)
    {
        // Bạn có thể viết thêm hàm GetEmployeeById trong Repository để check
        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
        {
            string query = "SELECT COUNT(1) FROM Employees WHERE EmployeeID = @ID AND Status != 'Resigned'";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", id);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}