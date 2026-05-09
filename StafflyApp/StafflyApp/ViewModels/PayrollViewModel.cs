using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using StafflyApp.Models;

namespace StafflyApp.ViewModels
{
    public partial class PayrollViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Payroll> _importedRecords = new();

        [ObservableProperty]
        private bool _isDataLoaded = false;

        [ObservableProperty]
        private string _filePath = "Chưa chọn file nào...";

        [RelayCommand]
        private void ImportExcel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                ReadExcelFile(FilePath);
                IsDataLoaded = ImportedRecords.Any();
            }
        }

        private void ReadExcelFile(string path)
        {
            ImportedRecords.Clear();
            ExcelPackage.License.SetNonCommercialPersonal("STAFFLY");

            try
            {
                using (var package = new ExcelPackage(new FileInfo(path)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var record = new Payroll();

                        var idRaw = worksheet.Cells[row, 1].Value;
                        var nameRaw = worksheet.Cells[row, 2].Value;
                        var salaryRaw = worksheet.Cells[row, 3].Value;
                        var bonusRaw = worksheet.Cells[row, 4].Value;

                        record.EmployeeName = nameRaw?.ToString() ?? "N/A";
                        record.Month = DateTime.Now.Month;
                        record.Year = DateTime.Now.Year;

                        if (idRaw == null || !int.TryParse(idRaw.ToString(), out int id))
                        {
                            record.IsValid = false;
                            record.ErrorNote += "Mã NV không hợp lệ. ";
                        }
                        else record.EmployeeID = id;

                        if (salaryRaw == null || !decimal.TryParse(salaryRaw.ToString(), out decimal salary))
                        {
                            record.IsValid = false;
                            record.ErrorNote += "Lương không hợp lệ. ";
                        }
                        else record.TotalSalary = salary;

                        if (bonusRaw != null && decimal.TryParse(bonusRaw.ToString(), out decimal bonus))
                        {
                            record.TotalBonus = bonus;
                        }

                        ImportedRecords.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đọc file Excel: {ex.Message}");
            }
        }
    }
}