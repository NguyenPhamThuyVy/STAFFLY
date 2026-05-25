using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StafflyApp.Models;

namespace StafflyApp.Data
{
    public class StafflyDbContext : DbContext
    {

        public StafflyDbContext() { } 
        public DbSet<Department> Departments { get; set; }
        public StafflyDbContext(DbContextOptions<StafflyDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DepartmentAttendance> DepartmentAttendances { get; set; }
        public DbSet<DepartmentPayrollStatus> DepartmentPayrollStatuses { get; set; }
        public DbSet<EmployeePayroll> EmployeePayrolls { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeID);
            modelBuilder.Entity<User>().HasKey(u => u.UserID);
            modelBuilder.Entity<Payroll>().HasKey(p => p.PayrollID);
            modelBuilder.Entity<Attendance>().HasKey(a => a.AttendanceID);
            modelBuilder.Entity<AuditLog>().HasKey(log => log.LogID);

            modelBuilder.Entity<EmployeePayroll>()
                .Property(p => p.TotalSalary)
                .HasComputedColumnSql("[BasicSalary] + [Bonuses] - [Deductions]");

            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentID = 1, DepartmentName = "Board of Directors", HeadcountLimit = 5 },
                new Department { DepartmentID = 2, DepartmentName = "IT & Technology Department", HeadcountLimit = 20 },
                new Department { DepartmentID = 3, DepartmentName = "Human Resources Department (HR)", HeadcountLimit = 15 },
                new Department { DepartmentID = 4, DepartmentName = "Marketing Department", HeadcountLimit = 25 },
                new Department { DepartmentID = 5, DepartmentName = "Accounting Department", HeadcountLimit = 10 }
            );

            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Username = "admin", Password = "123", RoleID = 1, RoleName = "Admin", EmployeeID = null, IsActive = true },
                new User { UserID = 2, Username = "manager", Password = "abc", RoleID = 2, RoleName = "Manager", EmployeeID = null, IsActive = true },
                new User { UserID = 3, Username = "staff", Password = "a1b2", RoleID = 3, RoleName = "Staff", EmployeeID = null, IsActive = true }
            );

        }
    }
}
