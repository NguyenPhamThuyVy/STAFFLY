using System.Linq;
using StafflyApp.Models;
using StafflyApp.Data;

namespace StafflyApp.Data
{
    public class UserRepository
    {
        private readonly StafflyDbContext _context;

        public UserRepository(StafflyDbContext context)
        {
            _context = context;
        }

        public User? AuthenticateUser(string username, string password)
        {
            // Kiểm tra: Khớp Username, Khớp Password và IsActive phải là true
            return _context.Users
                .FirstOrDefault(u => u.Username == username
                                && u.Password == password
                                && u.IsActive == true);
        }

        public Employee? GetEmployeeByUserId(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null && user.EmployeeID.HasValue)
            {
                return _context.Employees.Find(user.EmployeeID.Value);
            }
            return null;
        }
    }
}