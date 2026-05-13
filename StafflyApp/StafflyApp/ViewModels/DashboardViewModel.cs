using LiveCharts;
using LiveCharts.Wpf;
using StafflyApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
namespace StafflyApp.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public SeriesCollection StaffGrowthSeries { get; set; }
        public SeriesCollection SalaryPieSeries { get; set; }
        public List<string> MonthsLabels { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalPayroll { get; set; }

        public DashboardViewModel()
        {
            LoadAnalyticsData();
        }

        private void LoadAnalyticsData()
        {
            // 1. Giả lập/Truy vấn biến động nhân sự (Line Chart)
            StaffGrowthSeries = new SeriesCollection {
            new LineSeries {
                Title = "Employees",
                Values = new ChartValues<int> { 82, 85, 88, 90, 92, 95 },
                PointGeometry = DefaultGeometries.Circle,
                Stroke = System.Windows.Media.Brushes.DeepSkyBlue
            }
        };
            MonthsLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };

            // 2. Truy vấn quỹ lương theo phòng ban (Pie Chart)
            SalaryPieSeries = new SeriesCollection {
            new PieSeries { Title = "IT", Values = new ChartValues<double> { 5500 }, DataLabels = true },
            new PieSeries { Title = "HR", Values = new ChartValues<double> { 3200 }, DataLabels = true },
            new PieSeries { Title = "Marketing", Values = new ChartValues<double> { 4100 }, DataLabels = true },
            new PieSeries { Title = "Accounting", Values = new ChartValues<double> { 2800 }, DataLabels = true }
        };

            TotalEmployees = 95;
            TotalPayroll = 15600;
        }
    }
}