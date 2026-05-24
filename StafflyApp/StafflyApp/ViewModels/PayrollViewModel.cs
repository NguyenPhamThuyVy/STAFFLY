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

        // Lưu trữ chuỗi trạng thái quét từ bảng DepartmentPayrollStatuses lên ("Approved", "Rejected", hoặc "Pending")
        [ObservableProperty] private string _currentStatusString = string.Empty;

        // Lưu chuỗi nội dung giải thích lý do từ chối (bốc từ cột RejectReason của DB lên)
        [ObservableProperty] private string _rejectReasonMessage = string.Empty;

        // Bật/tắt nút bấm trên giao diện 
        [ObservableProperty] private bool _isActionAllowed = true;

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

        /// <summary>
        /// 🎯 CORE LOGIC STAFF: Tự động quản lý chu kỳ sống dữ liệu và khóa/mở khóa tính năng dựa trên bộ lọc
        /// </summary>
        private void LoadStaffPayrollOverview()
        {
            // RESET dữ liệu tạm trên giao diện để tránh rác dữ liệu từ phòng ban cũ nhảy sang phòng ban mới
            FilePath = "No file selected";
            ImportedRecords.Clear();
            ErrorList.Clear();
            IsDataLoaded = false;
            SuccessCount = 0;
            FailureCount = 0;

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
                    // Quét DB kiểm tra trạng thái phê duyệt của chu kỳ đang chọn
                    var statusRecord = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.DepartmentID == SelectedDepartment.DepartmentID
                                          && s.Month == SelectedMonth
                                          && s.Year == SelectedYear);

                    if (statusRecord != null)
                    {
                        CurrentStatusString = statusRecord.Status; // "Pending", "Approved", "Rejected"
                        RejectReasonMessage = statusRecord.RejectReason ?? string.Empty;

                        // KHÓA HÀNH ĐỘNG: Chỉ mở khóa cho Import/Submit lại khi trạng thái là Rejected (Bị từ chối)
                        // Nếu là Approved hoặc đang Pending chờ sếp duyệt thì KHÓA CỨNG không cho chỉnh sửa 
                        IsActionAllowed = (statusRecord.Status == "Rejected" || statusRecord.Status == "Declined");

                        // Nếu bảng lương đã được gửi hoặc duyệt, lôi dữ liệu chi tiết trong DB lên grid để xem lại
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
                            IsDataLoaded = true; // Bật lên để hiển thị DataGrid
                        }
                    }
                    else
                    {
                        // Chưa từng khởi tạo chu kỳ lương nào -> Mở khóa tự do cho Staff làm việc
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
        /// <summary>
        /// 🔍 AUTO-DETECT LOGIC FOR STAFF: Tự động quét và định vị chu kỳ lỗi/cần xử lý ngay khi vào trang
        /// </summary>
        private void AutoDetectStaffTargetSheet()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // 1. Tìm xem có bảng lương nào bị Manager "Rejected" hoặc "Declined" không để Staff sửa trước
                    var targetSheet = db.DepartmentPayrollStatuses
                        .FirstOrDefault(s => s.Status == "Rejected" || s.Status == "Declined");

                    // 2.  Nếu không có ai bị từ chối, tìm gói đang "Pending" chờ duyệt để xem lại tình trạng
                    if (targetSheet == null)
                    {
                        targetSheet = db.DepartmentPayrollStatuses.FirstOrDefault(s => s.Status == "Pending");
                    }

                    // 3. Thực hiện bốc bộ lọc tự động nếu tìm thấy bản ghi thỏa mãn
                    if (targetSheet != null)
                    {
                        var targetDept = Departments.FirstOrDefault(d => d.DepartmentID == targetSheet.DepartmentID);
                        if (targetDept != null)
                        {
                            // Kích hoạt Binding gán ngược lên UI ComboBox
                            SelectedMonth = targetSheet.Month;
                            SelectedYear = targetSheet.Year;
                            SelectedDepartment = targetDept; // Gán cái này sẽ tự kích hoạt hàm LoadStaffPayrollOverview() load dữ liệu lên luôn

                            return; // Định vị thành công, thoát hàm
                        }
                    }

                    // 💡 Fallback: Nếu hệ thống trống trải hoàn toàn, tự động đưa về chu kỳ thời gian thực
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

        // Tự động kích hoạt tải và đồng bộ lại luồng hiển thị mỗi khi Staff chỉnh ComboBox bộ lọc
        partial void OnSelectedMonthChanged(int value) => LoadStaffPayrollOverview();
        partial void OnSelectedYearChanged(int value) => LoadStaffPayrollOverview();
        partial void OnSelectedDepartmentChanged(Department? value) => LoadStaffPayrollOverview();

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

                    Application.Current.Dispatcher.Invoke(() =>
                    {
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
            if (!IsActionAllowed) return;
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

                // Chuyển màu Banner và khóa nút thao tác ngay lập tức
                CurrentStatusString = "Pending";
                IsActionAllowed = false;

                // Tải lại luồng hiển thị tổng thể để đồng nhất cấu trúc dữ liệu sạch
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