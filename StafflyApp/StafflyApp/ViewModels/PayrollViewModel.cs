using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Models;
using Microsoft.Win32;
using System.Linq;

namespace StafflyApp.ViewModels
{
    // Cần có chữ partial
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
                SimulateData(); // Hàm tạo dữ liệu mẫu để test bôi đỏ
                IsDataLoaded = ImportedRecords.Any();
            }
        }

        private void SimulateData()
        {
            ImportedRecords.Clear();
            ImportedRecords.Add(new Payroll { EmployeeID = 1, EmployeeName = "Quốc Thịnh", Month = 5, Year = 2026, TotalSalary = 15000000, IsValid = true });
            ImportedRecords.Add(new Payroll { EmployeeID = null, EmployeeName = "Lỗi Mã NV", Month = 5, Year = 2026, TotalSalary = 10000000, IsValid = false, ErrorNote = "Thiếu ID nhân viên" });
        }
    }
}