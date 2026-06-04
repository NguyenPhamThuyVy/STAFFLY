namespace StafflyApp.Models
{
    public class AttendanceImportModel
    {
        public int EmployeeID { get; set; }

        public string EmployeeName { get; set; }

        public DateTime Date { get; set; }
        public string Status { get; set; } 

        public string DepartmentName { get; set; }

        public int? DepartmentID { get; set; }

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}