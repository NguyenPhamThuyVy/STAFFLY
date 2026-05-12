using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StafflyApp.Models
{
    public class Payroll
    {
        public int PayrollID { get; set; }
        public int? EmployeeID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalSalary { get; set; }
        public string Status { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        // === CÁC TRƯỜNG BỔ SUNG CHỈ DÙNG CHO GIAO DIỆN IMPORT EXCEL ===
        public string EmployeeName { get; set; } // Tên nhân viên để hiển thị trên DataGrid
        public bool IsValid { get; set; } = true; // Cờ đánh dấu dòng bị lỗi
        public string ErrorNote { get; set; } // Câu thông báo lỗi (ví dụ: "Lương âm", "Sai mã NV")
    }
}
