using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ExcelDataReader; 

public class ExcelImporter
{
    public void ImportSalaryData(string filePath, string connectionString)
    {
        // 1. Đọc file Excel vào DataTable
        using (var stream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                DataTable excelTable = result.Tables[0];

                // 2. Sử dụng SqlBulkCopy để đẩy dữ liệu cực nhanh vào SQL Server
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        // Tên bảng đích trong SQL Server
                        bulkCopy.DestinationTableName = "dbo.TmpSalaryImport";

                        // Thiết lập kích thước gói dữ liệu gửi đi một lúc (tối ưu hiệu năng)
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 60; // 60 giây timeout

                        // Ánh xạ cột từ Excel (Source) sang SQL (Destination)
                        bulkCopy.ColumnMappings.Add("MaNhanVien", "EmployeeID");
                        bulkCopy.ColumnMappings.Add("LuongCoBan", "BaseSalary");
                        bulkCopy.ColumnMappings.Add("Thang", "SalaryMonth");

                        // Thực thi ghi vào Database
                        bulkCopy.WriteToServer(excelTable);
                    }
                }
            }
        }
    }
}