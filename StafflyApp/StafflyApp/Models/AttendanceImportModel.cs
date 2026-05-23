public class AttendanceImportModel
{
    public string DepartmentName { get; set; } // Map với cột Department Name
    public int AbsenceCount { get; set; }      // Map với cột Absences
    public int TardyCount { get; set; }        // Map với cột Tardy
    public bool IsValid { get; set; }          // Để hiện icon check xanh/đỏ
    public string ErrorMessage { get; set; }   // Để hiện dòng chữ lỗi khi rà chuột vào dòng bị sai
}