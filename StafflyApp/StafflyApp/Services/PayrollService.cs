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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var db = new StafflyDbContext())
            {
                // 1. CHỐT CHẶN: Kiểm tra trạng thái duyệt lương của phòng ban này trong tháng/năm đó
                var payrollStatus = db.DepartmentPayrollStatuses
                    .FirstOrDefault(s => s.DepartmentID == deptId && s.Month == month && s.Year == year);

                if (payrollStatus != null && payrollStatus.Status == "Approved")
                {
                    return $"Payroll for this department in {month}/{year} has already been APPROVED and locked. Re-import denied!";
                }

                // 2. NẾU BỊ TỪ CHỐI (Rejected): Xóa sạch dữ liệu lương cũ của phòng đó tháng đó để chuẩn bị ghi đè bản mới
                if (payrollStatus != null && payrollStatus.Status == "Rejected")
                {
                    var oldPayrolls = db.EmployeePayrolls
                        .Where(p => p.DepartmentID == deptId && p.Month == month && p.Year == year);
                    db.EmployeePayrolls.RemoveRange(oldPayrolls);
                }

                // 3. TIẾN HÀNH ĐỌC FILE EXCEL
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        var newPayrolls = new List<EmployeePayroll>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var idValue = worksheet.Cells[row, 1].Value;
                            var basicValue = worksheet.Cells[row, 2].Value;
                            var bonusValue = worksheet.Cells[row, 3].Value;
                            var deductValue = worksheet.Cells[row, 4].Value;

                            if (idValue != null && int.TryParse(idValue.ToString(), out int empId))
                            {
                                // Kiểm tra nhân viên đó có thực sự thuộc phòng ban đang import
                                var emp = db.Employees.FirstOrDefault(e => e.EmployeeID == empId && e.DepartmentID == deptId);
                                if (emp == null) continue; // Nếu không thuộc phòng này thì bỏ qua dòng đó

                                var payrollItem = new EmployeePayroll
                                {
                                    EmployeeID = empId,
                                    DepartmentID = deptId,
                                    Month = month,
                                    Year = year,
                                    BasicSalary = basicValue != null ? decimal.Parse(basicValue.ToString()!) : 0,
                                    Bonuses = bonusValue != null ? decimal.Parse(bonusValue.ToString()!) : 0,
                                    Deductions = deductValue != null ? decimal.Parse(deductValue.ToString()!) : 0
                                };
                                newPayrolls.Add(payrollItem);
                            }
                        }

                        if (newPayrolls.Count == 0) return "No valid employee payroll records found in the excel file.";

                        // Lưu chi tiết lương vào DB
                        db.EmployeePayrolls.AddRange(newPayrolls);

                        // 4. CẬP NHẬT HOẶC TẠO MỚI TRẠNG THÁI PHÊ DUYỆT THÀNH 'Pending'
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
                            payrollStatus.RejectReason = null; // Xóa lý do từ chối cũ đi vì đã nộp bản mới
                        }

                        db.SaveChanges();
                    }
                    return null; // Trả về null tức là import thành công 
                }
                catch (Exception ex)
                {
                    return "Excel Import Failed: " + ex.Message;
                }
            }
        }
    }
}