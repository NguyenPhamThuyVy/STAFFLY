namespace StafflyApp.Services
{
    using StafflyApp.Data;
    using StafflyApp.Models;

    public class EmployeeService
    {
        private readonly EmployeeRepository _repository;

        public EmployeeService()
        {
            _repository = new EmployeeRepository();
        }

        public string SaveEmployee(Employee emp)
        {
            // 1. Kiểm tra Email
            if (string.IsNullOrWhiteSpace(emp.Email) || !emp.Email.Contains("@"))
                return "Email không hợp lệ.";

            // 2. Kiểm tra tuổi
            if (emp.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - emp.DateOfBirth.Value.Year;
                if (age < 18) return "Nhân viên phải từ 18 tuổi.";
            }

            // 3. Nếu mọi thứ OK, gọi Repository để lưu
            bool success = _repository.AddEmployee(emp);

            return success ? null : "Lỗi khi lưu vào cơ sở dữ liệu.";
        }
    }
}