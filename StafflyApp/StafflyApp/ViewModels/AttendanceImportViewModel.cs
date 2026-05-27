using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StafflyApp.ViewModels
{
    public partial class AttendanceImportViewModel : ObservableObject
    {
        public event Action OnImportSuccess;

        [ObservableProperty] private string _excelFilePath = "No file selected...";
        [ObservableProperty] private bool _isDataLoaded = false;
        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        public ObservableCollection<AttendanceImportModel> ImportedAttendanceRecords { get; set; } = new();

        public AttendanceImportViewModel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("StafflyApp");
        }

        [RelayCommand]
        private async Task SelectExcelFile()
        {
            var ofd = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
            if (ofd.ShowDialog() == true)
            {
                ExcelFilePath = ofd.FileName;
                ImportedAttendanceRecords.Clear();
                await Task.Run(() => ReadExcelData(ExcelFilePath));
            }
        }

        private void ReadExcelData(string filePath)
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var allEmployees = db.Employees.Include(e => e.Department).ToList();

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int totalRows = worksheet.Dimension.Rows;

                        for (int row = 2; row <= totalRows; row++)
                        {
                            int.TryParse(worksheet.Cells[row, 1].Value?.ToString(), out int empId);
                            DateTime.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out DateTime date);
                            string status = worksheet.Cells[row, 4].Value?.ToString();

                            var employee = allEmployees.FirstOrDefault(e => e.EmployeeID == empId);

                            Application.Current.Dispatcher.Invoke(() => ImportedAttendanceRecords.Add(new AttendanceImportModel
                            {
                                EmployeeID = empId,
                                EmployeeName = employee?.FullName ?? "Unknown",
                                Date = date,
                                Status = status,
                                DepartmentName = employee?.Department?.DepartmentName ?? "Unknown",
                                IsValid = employee != null
                            }));
                        }
                    }
                }
                Application.Current.Dispatcher.Invoke(() => IsDataLoaded = ImportedAttendanceRecords.Any(r => r.IsValid));
            }
            catch (Exception ex) { MessageBox.Show("Error reading file: " + ex.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private void SaveAttendance()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var validRecords = ImportedAttendanceRecords.Where(r => r.IsValid).ToList();

                    if (validRecords.Any())
                    {
                        var targetMonths = validRecords.Select(r => new { r.Date.Month, r.Date.Year }).Distinct().ToList();

                        foreach (var target in targetMonths)
                        {
                            var oldRecords = db.Attendances.Where(a => a.Date.Month == target.Month && a.Date.Year == target.Year);
                            db.Attendances.RemoveRange(oldRecords);
                        }

                        foreach (var item in validRecords)
                        {
                            db.Attendances.Add(new Attendance
                            {
                                EmployeeID = item.EmployeeID,
                                Date = item.Date.Date,
                                Status = item.Status?.Trim() 
                            });
                        }

                        db.SaveChanges();
                    }
                }

                MessageBox.Show("Attendance records imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                ImportedAttendanceRecords.Clear();
                IsDataLoaded = false;

                OnImportSuccess?.Invoke();

                MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}