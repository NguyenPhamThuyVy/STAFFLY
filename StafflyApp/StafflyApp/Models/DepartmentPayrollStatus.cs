using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StafflyApp.Models
{
    public class DepartmentPayrollStatus
    {
        [Key]
        public int PayrollStatusID { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public int? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [StringLength(500)]
        public string? RejectReason { get; set; }

        // Navigation property (Nếu Entity Framework cần để map với bảng Departments)
        [ForeignKey("DepartmentID")]
        public virtual Department? Department { get; set; }
    }
}