using OfficeOpenXml;
using System.IO;
namespace StafflyApp.Business
{
    public class ExcelService
    {
        private readonly EmployeeRepository _repo = new EmployeeRepository();

        public List<string> ProcessPayroll(string filePath)
        {
            List<string> logs = new List<string>();
            // Lấy danh sách ID hiện có trong hệ thống để đối soát nhanh
            var existingIds = _repo.GetAllEmployees().Select(e => e.EmployeeID).ToHashSet();

            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Bắt đầu từ dòng 2 (bỏ header)
                {
                    var idCell = worksheet.Cells[row, 1].Value;
                    if (idCell != null && int.TryParse(idCell.ToString(), out int empId))
                    {
                        if (!existingIds.Contains(empId))
                        {
                            logs.Add($"Dòng {row}: Cảnh báo - EmployeeID {empId} không tồn tại trong hệ thống.");
                        }
                        else
                        {
                            logs.Add($"Dòng {row}: Khớp dữ liệu cho ID {empId}.");
                        }
                    }
                }
            }
            return logs;
        }
    }
}