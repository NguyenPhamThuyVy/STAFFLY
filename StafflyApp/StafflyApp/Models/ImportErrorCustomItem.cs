using System;

namespace StafflyApp.Models
{
    public class ImportErrorCustomItem
    {
        public int RowNumber { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ErrorDetail { get; set; } = string.Empty;
    }
}