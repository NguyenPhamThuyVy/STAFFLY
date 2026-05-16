public class PayrollReportDto
{
    public string EmployeeId { get; set; }
    public string FullName { get; set; }
    public string Position { get; set; }
    public string Department { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Allowance { get; set; }
    public int ActualWorkdays { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal TotalIncome => (BaseSalary / 22 * ActualWorkdays) + Allowance + OvertimePay;
    public decimal Insurance => TotalIncome * 0.105m; // 10.5% Người lao động đóng
    public decimal PersonalTax { get; set; }
    public decimal NetSalary => TotalIncome - Insurance - PersonalTax;
    public string Status { get; set; } = "Đã duyệt";
}