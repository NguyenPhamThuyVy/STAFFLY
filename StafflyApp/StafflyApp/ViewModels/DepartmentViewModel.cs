using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StafflyApp.ViewModels
{
    public partial class DepartmentViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Department> _departments = new();

        public DepartmentViewModel()
        {
            LoadDepartments();
        }

        [RelayCommand]
        private void LoadDepartments()
        {
            using (var db = new StafflyDbContext())
            {
                // Lấy danh sách phòng ban và bao gồm cả danh sách nhân viên để tính toán số lượng hiện tại
                var list = db.Departments.Include(d => d.Employees).ToList();
                Departments = new ObservableCollection<Department>(list);
            }
        }
    }
}