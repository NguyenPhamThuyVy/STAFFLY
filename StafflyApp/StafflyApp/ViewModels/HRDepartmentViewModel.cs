using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using OfficeOpenXml; // Thư viện đọc Excel EPPlus
using StafflyApp.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StafflyApp.ViewModels
{
    public partial class HRDepartmentViewModel : ObservableObject
    {
        // =======================================================
        // 1. BIẾN BINDING CHO BIỂU ĐỒ (CHART)
        // =======================================================
        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        [ObservableProperty] private SeriesCollection _attendanceExceptionsSeries = new();
        [ObservableProperty] private List<string> _departmentLabels = new();

        // Lắng nghe sự kiện thay đổi tháng/năm để Load lại biểu đồ
        partial void OnSelectedMonthChanged(int value) => LoadChartData();
        partial void OnSelectedYearChanged(int value) => LoadChartData();


        // =======================================================
        // 2. BIẾN BINDING CHO POPUP IMPORT EXCEL
        // =======================================================
        [ObservableProperty] private bool _isImportDialogOpen = false;

        [ObservableProperty] private int _importMonth = DateTime.Now.Month;
        [ObservableProperty] private int _importYear = DateTime.Now.Year;
        [ObservableProperty] private string _excelFilePath = string.Empty;
        [ObservableProperty] private bool _isDataLoaded = false;

        [ObservableProperty]
        private ObservableCollection<AttendanceImportModel> _importedAttendanceRecords = new();


        public HRDepartmentViewModel()
        {
            // Thiết lập bản quyền nội bộ cho EPPlus (Bắt buộc từ bản 5.x)
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("STAFFLY");

            LoadChartData();
        }

        // =======================================================
        // VẼ BIỂU ĐỒ (DÀNH CHO BACKEND CẦN MAP VỚI DATABASE)
        // =======================================================
        private void LoadChartData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Lấy danh sách phòng ban làm Trục X
                    var departments = db.Departments.ToList();
                    DepartmentLabels = departments.Select(d => d.DepartmentName).ToList();

                    var absencesValues = new ChartValues<int>();
                    var tardyValues = new ChartValues<int>();

                    // Lặp qua từng phòng ban, lấy tổng số vắng/trễ trong tháng/năm được chọn
                    foreach (var dept in departments)
                    {
                        // LƯU Ý CHO BACKEND: Thay 'DepartmentAttendances' bằng tên bảng thực tế lưu trữ dữ liệu điểm danh tổng hợp trong SQL
                        // Dưới đây là logic giả lập (Mock logic)

                        /* LƯỢC ĐỒ CHUẨN ĐỀ XUẤT CHO BACKEND:
                        var deptStats = db.DepartmentAttendances
                                          .FirstOrDefault(a => a.DepartmentID == dept.DepartmentID 
                                                            && a.Month == SelectedMonth 
                                                            && a.Year == SelectedYear);
                        
                        absencesValues.Add(deptStats?.TotalAbsences ?? 0);
                        tardyValues.Add(deptStats?.TotalTardiness ?? 0);
                        */

                        // Dữ liệu giả định để biểu đồ có hình chạy thử lên UI
                        absencesValues.Add(new Random().Next(0, 15));
                        tardyValues.Add(new Random().Next(5, 30));
                    }

                    // Khởi tạo 2 Series: Cột (Vắng) và Đường (Trễ)
                    AttendanceExceptionsSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Total Absences",
                            Values = absencesValues,
                            Fill = System.Windows.Media.Brushes.Crimson // Đỏ Crimson (#BE123C)
                        },
                        new LineSeries
                        {
                            Title = "Tardy Lateness",
                            Values = tardyValues,
                            Stroke = System.Windows.Media.Brushes.DarkOrange, // Cam (#D97706)
                            Fill = System.Windows.Media.Brushes.Transparent,
                            PointGeometrySize = 10
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chart Load Error: " + ex.Message);
            }
        }

        // =======================================================
        // CÁC LỆNH (COMMANDS) CHO POPUP IMPORT
        // =======================================================
        [RelayCommand]
        private void OpenImportDialog()
        {
            // Reset dữ liệu cũ trước khi mở
            ImportedAttendanceRecords.Clear();
            ExcelFilePath = string.Empty;
            IsDataLoaded = false;

            ImportMonth = DateTime.Now.Month;
            ImportYear = DateTime.Now.Year;

            IsImportDialogOpen = true;
        }

        [RelayCommand]
        private async Task SelectExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select Department Attendance Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExcelFilePath = openFileDialog.FileName;
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
                    var existingDepts = db.Departments.Select(d => d.DepartmentName.ToLower()).ToList();

                    FileInfo fileInfo = new FileInfo(filePath);
                    using (ExcelPackage package = new ExcelPackage(fileInfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        // Bắt đầu đọc từ dòng số 2 (Bỏ qua dòng Header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string deptName = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                            string rawAbsence = worksheet.Cells[row, 2].Value?.ToString() ?? "0";
                            string rawTardy = worksheet.Cells[row, 3].Value?.ToString() ?? "0";

                            bool isValid = true;
                            string errorMsg = "";

                            // Validation: Kiểm tra tên phòng ban có trong DB không
                            if (string.IsNullOrEmpty(deptName))
                            {
                                isValid = false; errorMsg += "Department name missing. ";
                            }
                            else if (!existingDepts.Contains(deptName.ToLower()))
                            {
                                isValid = false; errorMsg += $"Department '{deptName}' not found in system. ";
                            }

                            // Validation: Parse số liệu
                            if (!int.TryParse(rawAbsence, out int absences)) { isValid = false; errorMsg += "Invalid Absence number. "; }
                            if (!int.TryParse(rawTardy, out int tardy)) { isValid = false; errorMsg += "Invalid Tardy number. "; }

                            var record = new AttendanceImportModel
                            {
                                DepartmentName = deptName,
                                AbsenceCount = absences,
                                TardyCount = tardy,
                                IsValid = isValid,
                                ErrorMessage = errorMsg
                            };

                            // Đẩy lên UI (Vì dùng đa luồng nên cần invoke về UI Thread)
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ImportedAttendanceRecords.Add(record);
                            });
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsDataLoaded = ImportedAttendanceRecords.Any();
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error reading Excel file: " + ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        [RelayCommand]
        private void SaveAttendance()
        {
            var validRecords = ImportedAttendanceRecords.Where(r => r.IsValid).ToList();

            if (!validRecords.Any())
            {
                MessageBox.Show("There are no valid records to save. Please check the errors in the table.", "Validation Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new StafflyDbContext())
                {
                    var allDepts = db.Departments.ToList();

                    foreach (var item in validRecords)
                    {
                        var targetDept = allDepts.FirstOrDefault(d => d.DepartmentName.Equals(item.DepartmentName, StringComparison.OrdinalIgnoreCase));
                        if (targetDept == null) continue;

                        // LƯU Ý CHO BACKEND: Lưu xuống bảng thực tế
                        // var attendanceRecord = new DepartmentAttendance 
                        // { 
                        //     DepartmentID = targetDept.DepartmentID, 
                        //     Month = ImportMonth, 
                        //     Year = ImportYear, 
                        //     TotalAbsences = item.AbsenceCount, 
                        //     TotalTardiness = item.TardyCount 
                        // };
                        // db.DepartmentAttendances.Add(attendanceRecord);
                    }

                    // db.SaveChanges();

                    MessageBox.Show($"Successfully saved {validRecords.Count} department attendance records for {ImportMonth}/{ImportYear}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Tự động tải lại biểu đồ ngoài màn hình chính để thấy số liệu mới
                    SelectedMonth = ImportMonth;
                    SelectedYear = ImportYear;
                    LoadChartData();

                    // Đóng Popup Dialog (Nếu đóng không được bằng XAML tĩnh thì có thể bật dòng dưới lên)
                    IsImportDialogOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Save Error: " + ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // =======================================================
    // MODEL CHỨA DỮ LIỆU CỦA BẢNG DATA GRID TRONG POPUP
    // =======================================================
  
}