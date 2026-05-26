using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using StafflyApp.Data;
using StafflyApp.Models;

namespace StafflyApp.ViewModels
{
    public partial class HRDashboardViewModel : ObservableObject
    {
        // --- CÁC THUỘC TÍNH BINDING BỘ LỌC ---
        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        // --- CÁC CHỈ SỐ CARD TỔNG QUAN ---
        [ObservableProperty] private int _totalStaffCount;
        [ObservableProperty] private decimal _totalPayrollBudget;

        // --- CÁC THUỘC TÍNH ĐỔ SỐ LIỆU BIỂU ĐỒ (LIVECHARTS) ---
        [ObservableProperty] private SeriesCollection _payrollByDeptSeries = new();
        [ObservableProperty] private SeriesCollection _staffShareSeries = new();
        [ObservableProperty] private string[] _departmentLabels = Array.Empty<string>();

        // Định dạng hiển thị tiền tệ dạng "$ 1,500" trên trục tọa độ
        public Func<double, string> CurrencyFormatter { get; set; } = value => value.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

        public HRDashboardViewModel()
        {
            InitializeOrRefresh();
        }

        /// <summary>
        /// Hàm công khai để tầng View ép gọi làm mới dữ liệu
        /// Mỗi khi chuyển sang trang Dashboard, hàm này sẽ đảm bảo quét sạch dữ liệu cũ và cập nhật số liệu vừa Approve.
        /// </summary>
        public void InitializeOrRefresh()
        {
            RefreshDashboardData();
        }

        /// <summary>
        /// CORE LOGIC DASHBOARD: Hàm tính toán tổng hợp dữ liệu SQL đổ lên biểu đồ thời gian thực
        /// </summary>
        private void RefreshDashboardData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // 1. Thống kê Card 1: Tổng số lượng nhân sự đang hoạt động trong hệ thống
                    TotalStaffCount = db.Employees.Count();

                    // 2. Lấy danh sách tất cả phòng ban gốc để map nhãn
                    var allDepartments = db.Departments.ToList();

                    // 3. Truy vấn tính toán chi phí lương của TỪNG PHÒNG BAN trong tháng/năm được chọn
                    var approvedDeptIds = db.DepartmentPayrollStatuses
                        .Where(s => s.Month == SelectedMonth && s.Year == SelectedYear && s.Status == "Approved")
                        .Select(s => s.DepartmentID)
                        .ToList();

                    // Khởi tạo các mảng dữ liệu tạm thời
                    var labelsList = new List<string>();
                    var budgetValues = new ChartValues<decimal>();
                    var pieSeriesCollection = new SeriesCollection();

                    decimal runningTotalBudget = 0;

                    foreach (var dept in allDepartments)
                    {
                        labelsList.Add(dept.DepartmentName);
                        decimal deptTotalSalary = 0;
                        int deptStaffCountInPeriod = 0;

                        if (approvedDeptIds.Contains(dept.DepartmentID))
                        {
                            // 1. Tính tổng lương 
                            deptTotalSalary = db.EmployeePayrolls
                                .Where(ep => ep.DepartmentID == dept.DepartmentID && ep.Month == SelectedMonth && ep.Year == SelectedYear)
                                .Sum(ep => (decimal?)(ep.BasicSalary + ep.Bonuses - ep.Deductions)) ?? 0;

                            // 2. ĐẾM NHÂN SỰ ĐỘNG: Số dòng nhân viên được duyệt lương thực tế
                            deptStaffCountInPeriod = db.EmployeePayrolls
                                .Count(ep => ep.DepartmentID == dept.DepartmentID && ep.Month == SelectedMonth && ep.Year == SelectedYear);
                        }
                        else
                        {
                            // Fallback nếu tháng/năm đó chưa có dữ liệu lương được duyệt
                            deptStaffCountInPeriod = dept.CurrentStaffCount;
                        }

                        budgetValues.Add(deptTotalSalary);
                        runningTotalBudget += deptTotalSalary;

                        // Nạp dữ liệu vào biểu đồ tròn
                        pieSeriesCollection.Add(new PieSeries
                        {
                            Title = dept.DepartmentName,
                            Values = new ChartValues<int> { deptStaffCountInPeriod },
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{chartPoint.Y} staff ({chartPoint.Participation:P0})"
                        });
                    }

                    // 4. Cập nhật Card số 2: Tổng chi phí quỹ lương của tháng
                    TotalPayrollBudget = runningTotalBudget;

                    // 5. Cập nhật mảng nhãn phòng ban vào Trục X của biểu đồ cột đứng
                    DepartmentLabels = labelsList.ToArray();

                    // 6. THIẾT LẬP BIỂU ĐỒ CỘT ĐỨNG (ColumnSeries)
                    var verticalColumnSeries = new ColumnSeries
                    {
                        Title = "Total Payroll",
                        Values = budgetValues,
                        DataLabels = true,
                        FontSize = 12,
                        LabelPoint = point => point.Y > 0
                            ? point.Y.ToString("N0") + " USD"
                            : string.Empty,

                        Fill = System.Windows.Media.Brushes.RoyalBlue
                    };

                    PayrollByDeptSeries = new SeriesCollection { verticalColumnSeries };
                    StaffShareSeries = pieSeriesCollection;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RefreshDashboardData Error: " + ex.Message);
            }
        }

        // Tự động kích hoạt vẽ lại biểu đồ ngay lập tức mỗi khi Manager gạt chọn Tháng hoặc Năm khác
        partial void OnSelectedMonthChanged(int value) => RefreshDashboardData();
        partial void OnSelectedYearChanged(int value) => RefreshDashboardData();
    }
}