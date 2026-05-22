using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OfficeOpenXml; // Thư viện EPPlus
using StafflyApp.Data;
using StafflyApp.Data.Repositories;
using StafflyApp.Helpers;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StafflyApp.ViewModels
{
    public partial class PayrollViewModel : ObservableObject
    {
        [ObservableProperty] private string _filePath = "No file selected";
        [ObservableProperty] private bool _isDataLoaded = false;
        [ObservableProperty] private ObservableCollection<PayrollImportModel> _importedRecords = new();

        [ObservableProperty] private int _successCount;
        [ObservableProperty] private int _failureCount;
        [ObservableProperty] private ObservableCollection<ImportErrorItem> _errorList = new();

        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        [RelayCommand]
        private async Task ImportExcel()
        {
            int targetMonth = SelectedMonth;
            int targetYear = SelectedYear;

            // Chặn không cho import file lương nhiều lần trong cùng 1 tháng sau khi đã được accept
            try
            {
                using (var db = new StafflyDbContext())
                {
                    bool isLocked = db.Payrolls.Any(p => p.Month == targetMonth && p.Year == targetYear && p.Status == "Approved");
                    if (isLocked)
                    {
                        MessageBox.Show($"The payroll for Month {targetMonth}/{targetYear} has already been approved and locked! You cannot re-import data into this period.",
                                        "Payroll Locked", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking payroll lock status: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;

                // Reset dữ liệu trước khi nạp mới
                ImportedRecords.Clear();
                ErrorList.Clear();
                SuccessCount = 0;
                FailureCount = 0;

                try
                {
                    var tempList = new System.Collections.Generic.List<PayrollImportModel>();

                    await Task.Run(() =>
                    {
                        // Cú pháp chuẩn xác 100% cho EPPlus 8 trở lên
                        OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("STAFFLY");

                        FileInfo fileInfo = new FileInfo(FilePath);
                        using (ExcelPackage package = new ExcelPackage(fileInfo))
                        {
                            // Lấy Sheet đầu tiên
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            int rowCount = worksheet.Dimension.Rows;

                            // Chạy từ dòng 2 (bỏ qua Header)
                            for (int row = 2; row <= rowCount; row++)
                            {
                                // Đọc dữ liệu từ các ô (Cell)
                                string rawId = worksheet.Cells[row, 1].Value?.ToString();
                                string empName = worksheet.Cells[row, 2].Value?.ToString(); // Cột 2 là Tên
                                string rawSalary = worksheet.Cells[row, 3].Value?.ToString();
                                string rawBonus = worksheet.Cells[row, 4].Value?.ToString();

                                bool isRowValid = true;
                                string note = "";

                                // Validation logic 
                                if (string.IsNullOrWhiteSpace(empName)) { isRowValid = false; note += "Name is missing; "; }
                                if (!decimal.TryParse(rawSalary, out decimal salary)) { isRowValid = false; note += "Invalid Salary; "; }

                                var record = new PayrollImportModel
                                {
                                    EmployeeID = int.TryParse(rawId, out int id) ? id : 0,
                                    EmployeeName = empName ?? "Unknown",
                                    Month = targetMonth,
                                    Year = targetYear,
                                    TotalSalary = salary,
                                    TotalBonus = decimal.TryParse(rawBonus, out decimal bonus) ? bonus : 0,
                                    IsValid = isRowValid,
                                    ErrorNote = note
                                };
                                tempList.Add(record);
                            }
                        }
                    });

                    // Đổ dữ liệu vào UI
                    foreach (var item in tempList)
                    {
                        ImportedRecords.Add(item);
                        if (item.IsValid) SuccessCount++;
                        else
                        {
                            FailureCount++;
                            ErrorList.Add(new ImportErrorItem
                            {
                                RowNumber = ImportedRecords.Count,
                                EmployeeName = item.EmployeeName,
                                ErrorDetail = item.ErrorNote
                            });
                        }
                    }

                    IsDataLoaded = true;

                    // Hiện Dialog kết quả
                    var resultView = new Views.ImportResultView { DataContext = this };
                    await MaterialDesignThemes.Wpf.DialogHost.Show(resultView, "RootDialog");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading Excel: " + ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void SubmitToManager()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Chặn lần nữa
                    int targetMonth = SelectedMonth;
                    int targetYear = SelectedYear;

                    if (db.Payrolls.Any(p => p.Month == targetMonth && p.Year == targetYear && p.Status == "Approved"))
                    {
                        MessageBox.Show($"Submission Denied! The payroll for Month {targetMonth}/{targetYear} is locked.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }

                    var validRecords = ImportedRecords.Where(r => r.IsValid).ToList();

                    if (!validRecords.Any())
                    {
                        MessageBox.Show("No valid records to submit!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    foreach (var importItem in validRecords)
                    {
                        // KIỂM TRA TRÙNG TỪNG NHÂN VIÊN TRONG THÁNG/NĂM
                        var existingPayroll = db.Payrolls.FirstOrDefault(p => p.EmployeeID == importItem.EmployeeID && p.Month == targetMonth && p.Year == targetYear && p.Status == "Pending");

                        if (existingPayroll != null)
                        {
                            // Nếu đã có bản ghi Pending cũ của nhân viên này -> Cập nhật đè số tiền mới lên chứ không xóa nguyên bảng
                            existingPayroll.TotalSalary = importItem.TotalSalary;
                            existingPayroll.TotalBonus = importItem.TotalBonus;
                            existingPayroll.EmployeeName = importItem.EmployeeName;
                        }
                        else
                        {
                            // Nếu chưa có -> Thêm mới bản ghi cho nhân viên này
                            db.Payrolls.Add(new Payroll
                            {
                                EmployeeID = importItem.EmployeeID,
                                EmployeeName = importItem.EmployeeName,
                                Month = targetMonth,
                                Year = targetYear,
                                TotalSalary = importItem.TotalSalary,
                                TotalBonus = importItem.TotalBonus,
                                Status = "Pending"
                            });
                        }
                    }

                    if (db.SaveChanges() > 0)
                    {
                        // Ghi log chi tiết số lượng bản ghi nộp thành công, khớp chuẩn thời gian
                        UserRepository.LogAction(
                            UserSession.Instance.UserID,
                            "SUBMIT_PAYROLL",
                            $"Submitted payroll batch for Month {targetMonth}/{targetYear} (Total: {validRecords.Count} records successfully processed)."
                        );

                        MessageBox.Show($"Successfully submitted payroll records to HR Manager!",
                                        "Submission Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Reset trạng thái UI
                        IsDataLoaded = false;
                        FilePath = "No file selected";
                        ImportedRecords.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Save Error: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                                "Submission Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class ImportErrorItem
        {
            public int RowNumber { get; set; }
            public string EmployeeName { get; set; }
            public string ErrorDetail { get; set; }
        }
    }
}