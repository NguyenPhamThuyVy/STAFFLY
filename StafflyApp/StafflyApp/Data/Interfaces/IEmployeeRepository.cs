using StafflyApp.Models;

namespace StafflyApp.Data.Interfaces
{
    public interface IEmployeeRepository
    {
        bool AddEmployee(Employee emp);
        bool DeleteEmployee(int id);
        List<Employee> GetAllEmployees();
        bool UpdateEmployee(Employee emp);
    }
}