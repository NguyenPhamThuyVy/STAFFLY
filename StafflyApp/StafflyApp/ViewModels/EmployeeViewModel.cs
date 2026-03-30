//using System;
//using System.Collections.ObjectModel;
//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using StafflyApp.Models;
//using StafflyApp.Data;

//namespace StafflyApp.ViewModels
//{
//    public partial class EmployeeViewModel : ObservableObject
//    {
//        private readonly EmployeeRepository _repository;
//        [ObservableProperty]
//        private ObservableCollection<Employee> _employees = new();

//        [ObservableProperty]
//        private Employee? _selectedEmployee;
//        private readonly DepartmentRepository _deptRepository = new();
//        [ObservableProperty]
//        private ObservableCollection<Department> _departments = new();
//        public EmployeeViewModel()
//        {
//            _repository = new EmployeeRepository();
//            LoadData();
//        }

//        [RelayCommand]
//        private void LoadData()
//        {
//            var list = _repository.GetAllEmployees();
//            Employees = new ObservableCollection<Employee>(list);
//            var deptList = _deptRepository.GetAllDepartments();
//            Departments = new ObservableCollection<Department>(deptList);
//        }

//        [RelayCommand]
//        private void AddNewEmployee()
//        {
//            // Logic BE: tạm thời gọi LoadData để refresh
//            LoadData();
//        }

//        [RelayCommand]
//        private void DeleteSelected()
//        {
//            if (SelectedEmployee != null)
//            {
//                if (_repository.DeleteEmployee(SelectedEmployee.EmployeeID))
//                {
//                    LoadData();
//                }
//            }
//        }
//    }
////}