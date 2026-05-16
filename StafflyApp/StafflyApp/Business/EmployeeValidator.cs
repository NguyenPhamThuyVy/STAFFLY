using System.Text.RegularExpressions;

public class EmployeeValidator
{
    // Kiểm tra định dạng Email chuẩn name@domain.com
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        // Regex chuẩn cho định dạng email
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    // Kiểm tra độ tuổi (Ví dụ: phải từ 18 tuổi trở lên)
    public static bool IsValidAge(DateTime? dob)
    {
        if (!dob.HasValue) return false;

        int age = DateTime.Now.Year - dob.Value.Year;
        // Kiểm tra nếu chưa qua sinh nhật năm nay thì trừ 1 tuổi
        if (dob.Value.Date > DateTime.Now.AddYears(-age)) age--;

        return age >= 18;
    }
}