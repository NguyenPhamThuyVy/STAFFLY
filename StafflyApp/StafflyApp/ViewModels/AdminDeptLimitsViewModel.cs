using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StafflyApp.Data;
using StafflyApp.Models;
using StafflyApp.Data.Repositories;
using StafflyApp.Helpers;

namespace StafflyApp.ViewModels
{
    public partial class AdminDeptLimitsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Department> _adminDeptCapacityList = new();

        public AdminDeptLimitsViewModel() => LoadDeptLimitsData();

        [RelayCommand]
        public void LoadDeptLimitsData()
        {
            try
            {
                using (var db = new StafflyDbContext())
                {
                    AdminDeptCapacityList = new ObservableCollection<Department>(db.Departments.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SaveDeptLimitData(Department dept)
        {
            if (dept == null) return;
            try
            {
                using (var db = new StafflyDbContext())
                {
                    var dbDept = db.Departments.FirstOrDefault(d => d.DepartmentID == dept.DepartmentID);
                    if (dbDept != null && dbDept.HeadcountLimit != dept.HeadcountLimit)
                    {
                        if (dept.HeadcountLimit < dbDept.CurrentStaffCount)
                        {
                            MessageBox.Show($"Warning: Limit ({dept.HeadcountLimit}) cannot be less than current staff ({dbDept.CurrentStaffCount})!",
                                            "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LoadDeptLimitsData();
                            return;
                        }
                        int oldLimit = dbDept.HeadcountLimit;
                        dbDept.HeadcountLimit = dept.HeadcountLimit;
                        db.SaveChanges();
                        int currentUserId = UserSession.Instance.UserID;
                        UserRepository.LogAction(
                            currentUserId,
                            "UPDATE_DEPT_LIMIT",
                            $"Updated headcount limit for department '{dbDept.DepartmentName}' (ID: {dbDept.DepartmentID}) from {oldLimit} to {dept.HeadcountLimit} slots."
                        );
                        MessageBox.Show("Department capacity updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadDeptLimitsData();
            }
        }
    }
}