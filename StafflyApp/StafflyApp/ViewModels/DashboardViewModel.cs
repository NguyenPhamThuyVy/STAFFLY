using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using StafflyApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StafflyApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private string _headcountWarningMessage = string.Empty;
        [ObservableProperty] private bool _isHeadcountWarningVisible = false;

        [ObservableProperty] private int _totalStaffCount;
        [ObservableProperty] private decimal _totalPayrollBudget;
        [ObservableProperty] private List<string> _departmentLabels = new();

        public SeriesCollection PayrollByDeptSeries { get; set; } = new();
        public SeriesCollection StaffShareSeries { get; set; } = new();
        public Func<double, string> CurrencyFormatter { get; set; } = value => value.ToString("N0") + " USD";

        [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
        [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

        public DashboardViewModel()
        {
            LoadAnalyticsData();
        }

        private void LoadAnalyticsData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var fullDepartments = db.Departments
                        .Where(d => d.HeadcountLimit > 0 && d.CurrentStaffCount >= d.HeadcountLimit)
                        .Select(d => d.DepartmentName)
                        .ToList();

                    if (fullDepartments.Any())
                    {
                        string deptNames = string.Join(", ", fullDepartments);
                        HeadcountWarningMessage = $"CRITICAL ALERT: {deptNames} reached 100% headcount capacity!";
                        IsHeadcountWarningVisible = true;
                    }
                    else
                    {
                        HeadcountWarningMessage = string.Empty;
                        IsHeadcountWarningVisible = false;
                    }

                    TotalStaffCount = db.Employees.Count(e => e.Status == "Active" || e.Status == "Working");
                    TotalPayrollBudget = db.EmployeePayrolls
                        .Where(p => p.Month == SelectedMonth && p.Year == SelectedYear)
                        .Sum(p => (decimal?)p.TotalSalary) ?? 0;

                    var departments = db.Departments.ToList();
                    DepartmentLabels = departments.Select(d => d.DepartmentName).ToList();

                    StaffShareSeries.Clear();
                    foreach (var dept in departments)
                    {
                        StaffShareSeries.Add(new PieSeries
                        {
                            Title = dept.DepartmentName,
                            Values = new ChartValues<int> { dept.CurrentStaffCount },
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{chartPoint.Y} staff ({chartPoint.Participation:P0})"
                        });
                    }

                    PayrollByDeptSeries.Clear();
                    var payrollValues = new ChartValues<decimal>();
                    foreach (var dept in departments)
                    {
                        var deptPayroll = db.EmployeePayrolls
                            .Where(p => p.DepartmentID == dept.DepartmentID && p.Month == SelectedMonth && p.Year == SelectedYear)
                            .Sum(p => (decimal?)p.TotalSalary) ?? 0;
                        payrollValues.Add(deptPayroll);
                    }

                    PayrollByDeptSeries.Add(new ColumnSeries
                    {
                        Title = "Payroll Budget",
                        Values = payrollValues
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dashboard metrics: " + ex.Message);
            }
        }
    }
}