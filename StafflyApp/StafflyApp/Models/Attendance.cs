using StafflyApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Attendance
{
    [Key]
    public int AttendanceID { get; set; }

    [Required]
    public int EmployeeID { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Status { get; set; } // "Absent", "Late"

    public bool IsLocked { get; set; }

    [ForeignKey("EmployeeID")]
    public virtual Employee Employee { get; set; }
}