using StafflyApp.Models;

namespace StafflyApp.Data.Interfaces
{
    public interface IDepartmentRepository
    {
        List<Department> GetAllDepartments();
    }
}