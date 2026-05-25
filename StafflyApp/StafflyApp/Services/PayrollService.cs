using OfficeOpenXml;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace StafflyApp.Services
{
    public class PayrollService
    {
        // Hàm Import lương theo Phòng ban, Tháng, Năm
        public string? ImportPayrollExcel(string filePath, int deptId, int month, int year, int currentUserId)
        {
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("STAFFLY");

            using (var db = new StafflyDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var payrollStatus = db.DepartmentPayrollStatuses
                            .FirstOrDefault(s => s.DepartmentID == deptId && s.Month == month && s.Year == year);

                        if (payrollStatus != null && payrollStatus.Status == "Approved")
                        {
                            return $"Payroll for this department in {month}/{year} has already been APPROVED and locked. Re-import denied!";
                        }

                        // Làm sạch dữ liệu chi tiết cũ trước khi ghi đè bản mới
                        var oldPayrolls = db.EmployeePayrolls
                            .Where(p => p.DepartmentID == deptId && p.Month == month && p.Year == year)
                            .ToList();

                        if (oldPayrolls.Any())
                        {
                            db.EmployeePayrolls.RemoveRange(oldPayrolls);
                            db.SaveChanges();
                        }

                        var newPayrolls = new List<EmployeePayroll>();

                        using (var package = new ExcelPackage(new FileInfo(filePath)))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            int rowCount = worksheet.Dimension.Rows;
                            int columnCount = worksheet.Dimension.Columns;

                            // 🎯 ĐÃ SỬA: Quét tìm dòng tiêu đề thực tế (Tối đa 15 dòng đầu) để định vị cột động
                            int headerRow = 0;
                            for (int r = 1; r <= Math.Min(rowCount, 15); r++)
                            {
                                string? cellValue = worksheet.Cells[r, 1].Value?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(cellValue) && cellValue.Contains("Employee ID", StringComparison.OrdinalIgnoreCase))
                                {
                                    headerRow = r;
                                    break;
                                }
                            }

                            // Nếu không tìm thấy dòng tiêu đề chuẩn, fallback về dòng 1
                            if (headerRow == 0) headerRow = 1;

                            // Thiết lập chỉ số cột động dựa trên từ khóa tiêu đề (Khớp 100% với file Import)
                            int colBasic = 2;
                            int colBonus = 3;
                            int colDeduct = 4;

                            for (int c = 1; c <= columnCount; c++)
                            {
                                string? colHeader = worksheet.Cells[headerRow, c].Value?.ToString()?.Trim()?.ToLower();
                                if (string.IsNullOrEmpty(colHeader)) continue;

                                if (colHeader.Contains("basic")) colBasic = c;
                                else if (colHeader.Contains("bonus")) colBonus = c;
                                else if (colHeader.Contains("deduct")) colDeduct = c;
                            }

                            // Cấu hình định dạng đọc số tiền chuẩn quốc tế (Chấp nhận cả dấu phẩy hàng nghìn)
                            var style = System.Globalization.NumberStyles.Number | System.Globalization.NumberStyles.AllowCurrencySymbol | System.Globalization.NumberStyles.AllowThousands;
                            var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

                            // Đọc dữ liệu thật bắt đầu từ dòng sau dòng tiêu đề (headerRow + 1)
                            for (int row = headerRow + 1; row <= rowCount; row++)
                            {
                                var idValue = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                var basicValue = worksheet.Cells[row, colBasic].Value?.ToString()?.Trim();
                                var bonusValue = worksheet.Cells[row, colBonus].Value?.ToString()?.Trim();
                                var deductValue = worksheet.Cells[row, colDeduct].Value?.ToString()?.Trim();

                                // Bỏ qua dòng trống hoặc dòng tính tổng cộng cuối file Excel
                                if (string.IsNullOrWhiteSpace(idValue) || idValue.Contains("Total", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                if (int.TryParse(idValue, out int empId))
                                {
                                    // Kiểm tra nhân viên thuộc phòng ban
                                    var emp = db.Employees.FirstOrDefault(e => e.EmployeeID == empId && e.DepartmentID == deptId);
                                    if (emp == null) continue;

                                    if (newPayrolls.Any(p => p.EmployeeID == empId)) continue;

                                    // Ép kiểu an toàn, lỗi định dạng hoặc trống tự trả về 0 chứ không văng app
                                    decimal basic = decimal.TryParse(basicValue, style, culture, out decimal _basic) ? _basic : 0;
                                    decimal bonus = decimal.TryParse(bonusValue, style, culture, out decimal _bonus) ? _bonus : 0;
                                    decimal deduct = decimal.TryParse(deductValue, style, culture, out decimal _deduct) ? _deduct : 0;

                                    var payrollItem = new EmployeePayroll
                                    {
                                        EmployeeID = empId,
                                        DepartmentID = deptId,
                                        Month = month,
                                        Year = year,
                                        BasicSalary = basic,
                                        Bonuses = bonus,
                                        Deductions = deduct
                                        // Cột TotalSalary đã được SQL Server lo tự động bằng Computed Column rồi nhé!
                                    };
                                    newPayrolls.Add(payrollItem);
                                }
                            }

                            if (newPayrolls.Count == 0) return "No valid employee payroll records found in the excel file.";

                            db.EmployeePayrolls.AddRange(newPayrolls);
                            db.SaveChanges();

                            if (payrollStatus == null)
                            {
                                payrollStatus = new DepartmentPayrollStatus
                                {
                                    DepartmentID = deptId,
                                    Month = month,
                                    Year = year,
                                    Status = "Pending"
                                };
                                db.DepartmentPayrollStatuses.Add(payrollStatus);
                            }
                            else
                            {
                                payrollStatus.Status = "Pending";
                                payrollStatus.RejectReason = null;
                            }

                            db.SaveChanges();
                        }

                        transaction.Commit();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string innerMessage = ex.InnerException != null ? $"\nDetails: {ex.InnerException.Message}" : "";
                        return "Excel Import Failed: " + ex.Message + innerMessage;
                    }
                }
            }
        }
    }
}