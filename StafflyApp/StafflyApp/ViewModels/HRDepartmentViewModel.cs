using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using StafflyApp.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StafflyApp.ViewModels
{
    public partial class HRDepartmentViewModel : ObservableObject
    {
        private int _selectedMonth = DateTime.Now.Month;
        public int SelectedMonth { get => _selectedMonth; set { SetProperty(ref _selectedMonth, value); RefreshData(); } }

        private int _selectedYear = DateTime.Now.Year;
        public int SelectedYear { get => _selectedYear; set { SetProperty(ref _selectedYear, value); RefreshData(); } }

        [ObservableProperty] private SeriesCollection _attendanceExceptionsSeries = new();
        [ObservableProperty] private List<string> _departmentLabels = new();
        [ObservableProperty] private bool _isImportDialogOpen = false;

        // Dòng này cực quan trọng để kết nối với file Import mới
        public AttendanceImportViewModel ImportVM { get; set; } = new AttendanceImportViewModel();
        public ObservableCollection<dynamic> TopRiskyList { get; set; } = new();

        public class AttendancePoint { public int Value { get; set; } public string ComparisonText { get; set; } }

        public HRDepartmentViewModel()
        {
            LiveCharts.Charting.For<AttendancePoint>(Mappers.Xy<AttendancePoint>().X((p, i) => i).Y(p => p.Value));
            RefreshData();
        }

        private void RefreshData()
        {
            LoadChartData();
            LoadTopRiskyList();
        }

        private void LoadChartData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var departments = db.Departments.ToList();
                    DepartmentLabels = departments.Select(d => d.DepartmentName).ToList();

                    var absencesValues = new ChartValues<AttendancePoint>();
                    var tardyValues = new ChartValues<AttendancePoint>();

                    DateTime currentSelectedDate = new DateTime(SelectedYear, SelectedMonth, 1);

                    var mostRecentPastDate = db.Attendances
                        .Where(a => a.Date < currentSelectedDate)
                        .OrderByDescending(a => a.Date)
                        .Select(a => (DateTime?)a.Date)
                        .FirstOrDefault();

                    int lastMonth = 0;
                    int lastYear = 0;
                    bool hasPrevData = mostRecentPastDate.HasValue;

                    if (hasPrevData)
                    {
                        lastMonth = mostRecentPastDate.Value.Month;
                        lastYear = mostRecentPastDate.Value.Year;
                    }

                    foreach (var dept in departments)
                    {
                        var deptAttendances = db.Attendances.Include(a => a.Employee)
                                                .Where(a => a.Employee.DepartmentID == dept.DepartmentID);

                        int curA = deptAttendances.Count(a => a.Date.Month == SelectedMonth && a.Date.Year == SelectedYear && a.Status == "Absent");
                        int curL = deptAttendances.Count(a => a.Date.Month == SelectedMonth && a.Date.Year == SelectedYear && a.Status == "Late");

                        int preA = 0;
                        int preL = 0;

                        if (hasPrevData)
                        {
                            preA = deptAttendances.Count(a => a.Date.Month == lastMonth && a.Date.Year == lastYear && a.Status == "Absent");
                            preL = deptAttendances.Count(a => a.Date.Month == lastMonth && a.Date.Year == lastYear && a.Status == "Late");
                        }

                        string GetText(int cur, int pre, bool hasPrev, int prevMonth)
                        {
                            if (!hasPrev) return " (No prev data)";

                            string monthName = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(prevMonth);

                            if (cur == pre) return $" (Stable vs {monthName})";
                            int diff = cur - pre;
                            return $" ({(diff > 0 ? "+" : "")}{diff} vs {monthName})";
                        }

                        absencesValues.Add(new AttendancePoint { Value = curA, ComparisonText = GetText(curA, preA, hasPrevData, lastMonth) });
                        tardyValues.Add(new AttendancePoint { Value = curL, ComparisonText = GetText(curL, preL, hasPrevData, lastMonth) });
                    }

                    AttendanceExceptionsSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Total Absences",
                            Values = absencesValues,
                            Fill = System.Windows.Media.Brushes.Crimson,
                            LabelPoint = p => $"{((AttendancePoint)p.Instance).Value}{((AttendancePoint)p.Instance).ComparisonText}"
                        },
                        new LineSeries
                        {
                            Title = "Tardy Lateness",
                            Values = tardyValues,
                            Stroke = System.Windows.Media.Brushes.DarkOrange,
                            Fill = System.Windows.Media.Brushes.Transparent,
                            PointGeometrySize = 10,
                            LabelPoint = p => $"{((AttendancePoint)p.Instance).Value}{((AttendancePoint)p.Instance).ComparisonText}"
                        }
                    };
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }
        public void LoadTopRiskyList()
        {
            using (var db = new StafflyDbContext())
            {
                var top = db.Attendances.Include(a => a.Employee)
                    .Where(a => a.Date.Month == SelectedMonth && a.Date.Year == SelectedYear && (a.Status == "Absent" || a.Status == "Late"))
                    .GroupBy(a => a.Employee.FullName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count).Take(3).ToList();

                TopRiskyList.Clear();
                foreach (var item in top) TopRiskyList.Add(item);
            }
        }

        [RelayCommand] private void OpenImportDialog() => IsImportDialogOpen = true;
    }
}