using System;

namespace StafflyApp.Models
{
    public class PayrollImportModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal TotalSalary { get; set; }
        public bool IsValid { get; set; }
        public string ErrorNote { get; set; } = string.Empty;
    }
}