using System.ComponentModel.DataAnnotations;

namespace StafflyApp.Models
{
    public class DepartmentAttendance
    {
        [Key]
        public int Id { get; set; }
        public int DepartmentID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalAbsences { get; set; }
        public int TotalTardiness { get; set; }

        public virtual Department Department { get; set; }
    }
}