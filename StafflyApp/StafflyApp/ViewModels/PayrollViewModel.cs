using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StafflyApp.Models;
using System;
using StafflyApp.Data;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using OfficeOpenXml; // Thư viện EPPlus

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

        [RelayCommand]
        private async Task ImportExcel()
        {
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

                                // Validation logic (Việc 4)
                                if (string.IsNullOrWhiteSpace(empName)) { isRowValid = false; note += "Name is missing; "; }
                                if (!decimal.TryParse(rawSalary, out decimal salary)) { isRowValid = false; note += "Invalid Salary; "; }

                                var record = new PayrollImportModel
                                {
                                    EmployeeID = int.TryParse(rawId, out int id) ? id : 0,
                                    EmployeeName = empName ?? "Unknown", // <--- LẤY TÊN THẬT TỪ EXCEL
                                    Month = DateTime.Now.Month,
                                    Year = DateTime.Now.Year,
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
                    // Chỉ lọc ra các dòng dữ liệu hợp lệ (IsValid == true) để trình lên Sếp
                    var validRecords = ImportedRecords.Where(r => r.IsValid).ToList();

                    if (!validRecords.Any())
                    {
                        MessageBox.Show("No valid records to submit!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    foreach (var importItem in validRecords)
                    {
                        // Khởi tạo thực thể Payroll thật để chuẩn bị lưu xuống DB
                        var payrollDbRecord = new Payroll
                        {
                            EmployeeID = importItem.EmployeeID,
                            Month = importItem.Month,
                            Year = importItem.Year,
                            TotalSalary = importItem.TotalSalary,
                            TotalBonus = importItem.TotalBonus,
                            Status = "Pending", // GÁN TRẠNG THÁI CHỜ DUYỆT CỐT LÕI
                            RejectReason = string.Empty
                        };
                        db.Payrolls.Add(payrollDbRecord);
                    }

                    // Thực hiện lưu hàng loạt (Bulk Insert Optimization)
                    if (db.SaveChanges() > 0)
                    {
                        MessageBox.Show($"Successfully submitted {validRecords.Count} payroll records to HR Manager for review!",
                                        "Submission Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Reset trạng thái UI sau khi trình sếp thành công
                        IsDataLoaded = false;
                        FilePath = "No file selected";
                        ImportedRecords.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Submission Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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