using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Helpers;
using StafflyApp.Models;
using StafflyApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StafflyApp.ViewModels
{
    /// <summary>
    /// 📄 VIEWMODEL QUẢN LÝ TIẾN TRÌNH LUỒNG LƯƠNG (PAYROLL PIPELINE ENGINE)
    /// Đảm nhiệm việc đọc tệp Excel xem trước, chặn đệ trình khi phát sinh lỗi thực thể.
    /// </summary>
    public partial class PayrollViewModel : ObservableObject
    {
        private readonly PayrollService _payrollService = new PayrollService();

        [ObservableProperty] private string _filePath = "No file selected";
        [ObservableProperty] private bool _isDataLoaded = false;

        // Lưu trữ danh sách dữ liệu thô đọc từ Excel để hiển thị Preview DataGrid lên giao diện
        [ObservableProperty] private ObservableCollection<PayrollImportModel> _importedRecords = new();

        [ObservableProperty] private int _successCount;
        [ObservableProperty] private int _failureCount;

        [ObservableProperty] private ObservableCollection<ImportErrorCustomItem> _errorList = new();

        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        [ObservableProperty] private ObservableCollection<Department> _departments = new();
        [ObservableProperty] private Department? _selectedDepartment;

        [ObservableProperty] private string _currentStatusString = string.Empty;
        [ObservableProperty] private string _rejectReasonMessage = string.Empty;

        // Cờ cấu hình luồng: Cho phép thao tác Import file hay không (Approved/Pending sẽ khóa)
        [ObservableProperty] private bool _isActionAllowed = true;

        // Cờ kiểm soát điều kiện bấm nút đệ trình (Submit) lên cấp quản lý
        // Nút bấm gửi đi chỉ hoạt động khi file được load lên xem trước thành công VÀ hoàn toàn không có dòng nào lỗi
        [ObservableProperty] private bool _isSubmitAllowed = false;

        public PayrollViewModel()
        {
            _ = LoadDepartments();
        }

        private async Task LoadDepartments()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var list = await Task.Run(() => db.Departments.ToList());
                    Departments.Clear();
                    foreach (var dept in list) Departments.Add(dept);

                    if (Departments.Any())
                    {
                        AutoDetectStaffTargetSheet();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load departments: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStaffPayrollOverview()
        {
            FilePath = "No file selected";
            ImportedRecords.Clear();
            ErrorList.Clear();
            IsDataLoaded = false;
            SuccessCount = 0;
            FailureCount = 0;
            IsSubmitAllowed = false; // Reset trạng thái chặn Submit ban đầu

            if (SelectedDepartment == null || SelectedMonth < 1 || SelectedMonth > 12 || SelectedYear < 2000)
            {
                CurrentStatusString = string.Empty;
                RejectReasonMessage = string.Empty;
                IsActionAllowed = true;
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var statusRecord = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.DepartmentID == SelectedDepartment.DepartmentID
                                          && s.Month == SelectedMonth
                                          && s.Year == SelectedYear);

                    if (statusRecord != null)
                    {
                        CurrentStatusString = statusRecord.Status;
                        RejectReasonMessage = statusRecord.RejectReason ?? string.Empty;

                        IsActionAllowed = (statusRecord.Status == "Rejected" || statusRecord.Status == "Declined");

                        if (statusRecord.Status == "Pending" || statusRecord.Status == "Approved")
                        {
                            var savedDetails = (from ep in db.EmployeePayrolls
                                                join emp in db.Employees on ep.EmployeeID equals emp.EmployeeID
                                                where ep.DepartmentID == SelectedDepartment.DepartmentID
                                                   && ep.Month == SelectedMonth
                                                   && ep.Year == SelectedYear
                                                select new PayrollImportModel
                                                {
                                                    EmployeeID = ep.EmployeeID,
                                                    EmployeeName = emp.FullName,
                                                    Month = ep.Month,
                                                    Year = ep.Year,
                                                    BasicSalary = ep.BasicSalary,
                                                    TotalBonus = ep.Bonuses,
                                                    Deductions = ep.Deductions,
                                                    TotalSalary = ep.BasicSalary + ep.Bonuses - ep.Deductions,
                                                    IsValid = true,
                                                    ErrorNote = string.Empty
                                                }).ToList();

                            foreach (var item in savedDetails) ImportedRecords.Add(item);
                            IsDataLoaded = true;
                            IsSubmitAllowed = false; // Đã lưu vào DB rồi thì không cần cho Submit đè nữa
                        }
                    }
                    else
                    {
                        CurrentStatusString = string.Empty;
                        RejectReasonMessage = string.Empty;
                        IsActionAllowed = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadStaffPayrollOverview Error: " + ex.Message);
            }
        }

        private void AutoDetectStaffTargetSheet()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var targetSheet = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.Status == "Rejected" || s.Status == "Declined");

                    if (targetSheet == null)
                    {
                        targetSheet = db.DepartmentPayrollStatuses.FirstOrDefault(s => s.Status == "Pending");
                    }
                    if (targetSheet == null)
                    {
                        targetSheet = db.DepartmentPayrollStatuses
                            .OrderByDescending(s => s.Year)
                            .ThenByDescending(s => s.Month)
                            .FirstOrDefault(s => s.Status == "Approved");
                    }

                    if (targetSheet != null)
                    {
                        var targetDept = Departments.FirstOrDefault(d => d.DepartmentID == targetSheet.DepartmentID);
                        if (targetDept != null)
                        {
                            SelectedMonth = targetSheet.Month;
                            SelectedYear = targetSheet.Year;
                            SelectedDepartment = targetDept;
                            return;
                        }
                    }

                    if (SelectedMonth == 0) SelectedMonth = DateTime.Now.Month;
                    if (SelectedYear == 0) SelectedYear = DateTime.Now.Year;
                    if (SelectedDepartment == null && Departments.Any()) SelectedDepartment = Departments.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AutoDetectStaffTargetSheet Error: " + ex.Message);
            }
        }

        partial void OnSelectedMonthChanged(int value) => LoadStaffPayrollOverview();
        partial void OnSelectedYearChanged(int value) => LoadStaffPayrollOverview();
        partial void OnSelectedDepartmentChanged(Department? value) => LoadStaffPayrollOverview();

        /// <summary>
        /// 📥 HÀM NHẬP EXCEL XEM TRƯỚC (CHƯA LƯU XUỐNG SQL SERVER)
        /// Phân tích tệp, bốc tách dữ liệu thô đẩy lên DataGrid xem trước để rà soát lỗi chính tả/mã ID.
        /// </summary>
        [RelayCommand]
        private async Task ImportExcel()
        {
            if (!IsActionAllowed)
            {
                MessageBox.Show("This action is prohibited because the current payroll sheet is locked or currently pending approval!",
                                "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (SelectedDepartment == null)
            {
                MessageBox.Show("Please select a target department before selecting the Excel file!",
                                "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int targetMonth = SelectedMonth;
            int targetYear = SelectedYear;
            int targetDeptId = SelectedDepartment.DepartmentID;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select Payroll Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;

                ImportedRecords.Clear();
                ErrorList.Clear();
                IsDataLoaded = false;
                IsSubmitAllowed = false; // Tạm khóa khi đang quét dữ liệu

                int localSuccess = 0;
                int localFailure = 0;
                var localErrorList = new List<ImportErrorCustomItem>();
                var tempList = new List<PayrollImportModel>();

                try
                {
                    await Task.Run(() =>
                    {
                        OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("STAFFLY");
                        FileInfo fileInfo = new FileInfo(FilePath);
                        using (ExcelPackage package = new ExcelPackage(fileInfo))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            int rowCount = worksheet.Dimension.Rows;
                            int columnCount = worksheet.Dimension.Columns;

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

                            if (headerRow == 0)
                            {
                                localFailure++;
                                localErrorList.Add(new ImportErrorCustomItem
                                {
                                    RowNumber = 1,
                                    EmployeeName = "System",
                                    ErrorDetail = "Template Error: Could not find 'Employee ID' header column!"
                                });
                                return;
                            }

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

                            using (var db = new StafflyDbContext())
                            {
                                for (int row = headerRow + 1; row <= rowCount; row++)
                                {
                                    string? rawId = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                    string? rawBasic = worksheet.Cells[row, colBasic].Value?.ToString()?.Trim();
                                    string? rawBonus = worksheet.Cells[row, colBonus].Value?.ToString()?.Trim();
                                    string? rawDeduct = worksheet.Cells[row, colDeduct].Value?.ToString()?.Trim();

                                    if (string.IsNullOrWhiteSpace(rawId) || rawId.Contains("Total", StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    bool isRowValid = true;
                                    string note = "";
                                    string empNameFromDb = "Unknown";

                                    var style = System.Globalization.NumberStyles.Number | System.Globalization.NumberStyles.AllowCurrencySymbol | System.Globalization.NumberStyles.AllowThousands;
                                    var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

                                    if (!int.TryParse(rawId, out int empId)) { isRowValid = false; note += "Invalid Employee ID; "; }
                                    if (!decimal.TryParse(rawBasic, style, culture, out decimal basicSalary)) { isRowValid = false; note += "Invalid Basic Salary; "; }

                                    decimal bonus = decimal.TryParse(rawBonus, style, culture, out decimal b) ? b : 0;
                                    decimal deduction = decimal.TryParse(rawDeduct, style, culture, out decimal d) ? d : 0;

                                    if (isRowValid)
                                    {
                                        var employeeInDb = db.Employees.FirstOrDefault(e => e.EmployeeID == empId && e.DepartmentID == targetDeptId);

                                        if (employeeInDb == null)
                                        {
                                            isRowValid = false;
                                            note += $"Employee ID {empId} does not exist or belong to {SelectedDepartment.DepartmentName}; ";
                                        }
                                        else
                                        {
                                            empNameFromDb = employeeInDb.FullName;
                                        }
                                    }

                                    var record = new PayrollImportModel
                                    {
                                        EmployeeID = empId,
                                        EmployeeName = empNameFromDb,
                                        Month = targetMonth,
                                        Year = targetYear,
                                        BasicSalary = basicSalary,
                                        TotalBonus = bonus,
                                        Deductions = deduction,
                                        TotalSalary = basicSalary + bonus - deduction,
                                        IsValid = isRowValid,
                                        ErrorNote = note
                                    };
                                    tempList.Add(record);

                                    if (isRowValid) localSuccess++;
                                    else
                                    {
                                        localFailure++;
                                        localErrorList.Add(new ImportErrorCustomItem
                                        {
                                            RowNumber = row,
                                            EmployeeName = record.EmployeeName,
                                            ErrorDetail = record.ErrorNote
                                        });
                                    }
                                }
                            }
                        }
                    });

                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        SuccessCount = localSuccess;
                        FailureCount = localFailure;

                        foreach (var item in tempList) ImportedRecords.Add(item);
                        foreach (var err in localErrorList) ErrorList.Add(err);

                        IsDataLoaded = true;

                        // THIẾT LẬP QUY LUẬT KHÓA NÚT SUBMIT:
                        // Chỉ cho phép kích hoạt nút Submit gửi sếp khi có dữ liệu xem trước VÀ tổng số dòng lỗi phải bằng 0!
                        IsSubmitAllowed = (ImportedRecords.Any() && FailureCount == 0);

                        // Hiển thị bảng tóm tắt kết quả
                        var resultView = new Views.ImportResultView { DataContext = this };
                        await MaterialDesignThemes.Wpf.DialogHost.Show(resultView, "RootDialog");

                        // CẢNH BÁO CHO USER NẾU CÓ DÒNG LỖI CHẶN SUBMIT
                        if (FailureCount > 0)
                        {
                            MessageBox.Show($"This Excel file contains {FailureCount} invalid record(s) highlighted in RED!\n" +
                                            $"Submission to manager is STOCKED until you correct all errors in the file and re-import.",
                                            "Validation Error Detected", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading Excel file: " + ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 📤 HÀM CHÍNH THỨC GHI FILE VÀO DATABASE VÀ GỬI CHO SẾP DUYỆT
        /// Chỉ được gọi khi file xem trước hoàn toàn sạch lỗi. Tiến hành đổ dữ liệu xuống Database.
        /// </summary>
        [RelayCommand]
        private void SubmitToManager()
        {
            // Bảo vệ luồng nghiệp vụ nghiêm ngặt
            if (!IsActionAllowed) return;

            // CHẶN ĐỨNG HÀNH ĐỘNG NẾU FILE CÓ DÒNG LỖI
            if (FailureCount > 0 || !ImportedRecords.Any())
            {
                MessageBox.Show("Cannot submit! Please ensure the file has been imported preview and contains 0 errors.",
                                "Submission Rejected", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            int targetMonth = SelectedMonth;
            int targetYear = SelectedYear;
            int targetDeptId = SelectedDepartment.DepartmentID;
            int currentUserId = UserSession.Instance.UserID;

            // Tiến hành gọi Service để thực thi băm dữ liệu đổ xuống Database SQL Server
            string? errorResult = _payrollService.ImportPayrollExcel(FilePath, targetDeptId, targetMonth, targetYear, currentUserId);

            if (errorResult != null)
            {
                MessageBox.Show(errorResult, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // Ghi nhận nhật ký hệ thống (Audit Log)
                UserRepository.LogAction(
                    currentUserId,
                    "SUBMIT_PAYROLL",
                    $"Submitted payroll sheet for department '{SelectedDepartment.DepartmentName}' (ID: {targetDeptId}) for period {targetMonth}/{targetYear}. Total valid: {SuccessCount} records."
                );

                MessageBox.Show($"Successfully committed payroll records to database and submitted for {SelectedDepartment.DepartmentName} to the HR Manager!\nStatus is now set to 'Pending Approval'.",
                                "Submission Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                CurrentStatusString = "Pending";
                IsActionAllowed = false;
                IsSubmitAllowed = false; // Khóa nút sau khi nộp thành công

                LoadStaffPayrollOverview();
            }
        }

        [RelayCommand]
        private void DownloadTemplate()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Staffly_Payroll_Template.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("STAFFLY");
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Payroll Template");
                        worksheet.Cells[1, 1].Value = "Employee ID";
                        worksheet.Cells[1, 2].Value = "Basic Salary";
                        worksheet.Cells[1, 3].Value = "Bonuses";
                        worksheet.Cells[1, 4].Value = "Deductions";

                        worksheet.Cells.AutoFitColumns();
                        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                    }
                    MessageBox.Show("Template downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to export template: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}