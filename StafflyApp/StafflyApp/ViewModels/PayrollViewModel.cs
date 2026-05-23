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
    public partial class PayrollViewModel : ObservableObject
    {
        private readonly PayrollService _payrollService = new PayrollService();

        [ObservableProperty] private string _filePath = "No file selected";
        [ObservableProperty] private bool _isDataLoaded = false;

        [ObservableProperty] private ObservableCollection<PayrollImportModel> _importedRecords = new();

        [ObservableProperty] private int _successCount;
        [ObservableProperty] private int _failureCount;

        [ObservableProperty] private ObservableCollection<ImportErrorCustomItem> _errorList = new();

        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        [ObservableProperty] private ObservableCollection<Department> _departments = new();
        [ObservableProperty] private Department? _selectedDepartment;

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load departments: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ImportExcel()
        {
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Please select a target department before selecting the Excel file!",
                                "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int targetMonth = SelectedMonth;
            int targetYear = SelectedYear;
            int targetDeptId = SelectedDepartment.DepartmentID;

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var status = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.DepartmentID == targetDeptId && s.Month == targetMonth && s.Year == targetYear);

                    if (status != null && status.Status == "Approved")
                    {
                        MessageBox.Show($"The payroll for {SelectedDepartment.DepartmentName} in Month {targetMonth}/{targetYear} has already been APPROVED and locked!\nRe-import is denied.",
                                        "Payroll Locked", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking department payroll status: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select Payroll Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;

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

                            // Quét tìm dòng tiêu đề thực tế (Quét tối đa 15 dòng đầu)
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

                            // Định vị chỉ số cột động dựa trên tên tiêu đề tại headerRow
                            int colBasic = 2; // Giá trị fallback mặc định
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
                                // Đọc dữ liệu thật xuất phát từ dòng (headerRow + 1)
                                for (int row = headerRow + 1; row <= rowCount; row++)
                                {
                                    string? rawId = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                    string? rawBasic = worksheet.Cells[row, colBasic].Value?.ToString()?.Trim();
                                    string? rawBonus = worksheet.Cells[row, colBonus].Value?.ToString()?.Trim();
                                    string? rawDeduct = worksheet.Cells[row, colDeduct].Value?.ToString()?.Trim();

                                    // Bỏ qua dòng trống, dòng tổng cộng hoặc ghi chú cuối file của nhân sự
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
                                        // Tính tổng lương thực nhận nháp hiển thị
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

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ImportedRecords.Clear();
                        ErrorList.Clear();

                        SuccessCount = localSuccess;
                        FailureCount = localFailure;

                        foreach (var item in tempList) ImportedRecords.Add(item);
                        foreach (var err in localErrorList) ErrorList.Add(err);

                        IsDataLoaded = true;
                    });

                    var resultView = new Views.ImportResultView { DataContext = this };
                    await MaterialDesignThemes.Wpf.DialogHost.Show(resultView, "RootDialog");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading Excel file: " + ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void SubmitToManager()
        {
            if (SelectedDepartment == null || !ImportedRecords.Any()) return;

            int targetMonth = SelectedMonth;
            int targetYear = SelectedYear;
            int targetDeptId = SelectedDepartment.DepartmentID;
            int currentUserId = UserSession.Instance.UserID;

            var validRecords = ImportedRecords.Where(r => r.IsValid).ToList();
            if (!validRecords.Any())
            {
                MessageBox.Show("There are no valid employee records in the list to submit!", "Submission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? errorResult = _payrollService.ImportPayrollExcel(FilePath, targetDeptId, targetMonth, targetYear, currentUserId);

            if (errorResult != null)
            {
                MessageBox.Show(errorResult, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                UserRepository.LogAction(
                    currentUserId,
                    "SUBMIT_PAYROLL",
                    $"Submitted payroll sheet for department '{SelectedDepartment.DepartmentName}' (ID: {targetDeptId}) for period {targetMonth}/{targetYear}. Total valid: {validRecords.Count} records."
                );

                MessageBox.Show($"Successfully submitted payroll records for {SelectedDepartment.DepartmentName} to the HR Manager!\nStatus is now set to 'Pending Approval'.",
                                "Submission Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                IsDataLoaded = false;
                FilePath = "No file selected";
                ImportedRecords.Clear();
            }
        }
    }
}