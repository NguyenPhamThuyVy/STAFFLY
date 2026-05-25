using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace StafflyApp.ViewModels
{
    public partial class AdminAuditLogsViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<AuditLog> _logsList = new();
        [ObservableProperty] private string _searchText = string.Empty;

        // Danh sách lưu trữ log gốc để phục vụ bộ lọc tìm kiếm nhanh không cần load lại DB
        private System.Collections.Generic.List<AuditLog> _allLogsRaw = new();

        public AdminAuditLogsViewModel()
        {
            LoadSystemLogs();
        }

        // Hàm bốc dữ liệu Log từ SQL Server lên DataGrid
        [RelayCommand]
        public void LoadSystemLogs()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    // Lấy toàn bộ log, sắp xếp theo thời gian mới nhất (Timestamp giảm dần)
                    _allLogsRaw = db.AuditLogs
                                    .OrderByDescending(log => log.Timestamp)
                                    .ToList();

                    // Đổ dữ liệu ra UI công khai
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading audit logs: {ex.Message}", "Database Error",
                                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Logic bộ lọc tìm kiếm nhanh theo Loại Hành Động (Action) hoặc Chi tiết (Detail)
        [RelayCommand]
        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LogsList = new ObservableCollection<AuditLog>(_allLogsRaw);
            }
            else
            {
                string keyword = SearchText.Trim().ToLower();
                var filtered = _allLogsRaw.Where(log =>
                    (log.Action != null && log.Action.ToLower().Contains(keyword)) ||
                    (log.Detail != null && log.Detail.ToLower().Contains(keyword))
                ).ToList();

                LogsList = new ObservableCollection<AuditLog>(filtered);
            }
        }

        // Hàm xóa sạch ô tìm kiếm
        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            ApplyFilter();
        }
    }
}