using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StafflyApp.Models
{
    public class EmployeePayroll
    {
        [Key]
        public int PayrollID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonuses { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }

        // Cột tính toán tự động trong SQL Server nên ở C# chỉ cần thuộc tính read-only lấy giá trị 
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalSalary { get; private set; }
    }
}