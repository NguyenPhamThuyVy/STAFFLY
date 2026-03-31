using StafflyApp.Models;

namespace StafflyApp.Data.Interfaces
{
    public interface IUserRepository
    {
        User? AuthenticateUser(string username, string password);
        Employee? GetEmployeeByUserId(int userId);
    }
}