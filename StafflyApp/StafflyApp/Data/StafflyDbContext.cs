using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StafflyApp.Models;
using Microsoft.Extensions.Configuration; 

namespace StafflyApp.Data
{
    public class StafflyDbContext : DbContext
    {
        // Trong file StafflyDbContext.cs

        public StafflyDbContext() { } // Thêm dòng này
        public DbSet<Department> Departments { get; set; }
        public StafflyDbContext(DbContextOptions<StafflyDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Khởi tạo cấu hình để đọc từ file appsettings.json
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                // Lấy chuỗi kết nối từ file json
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Primary Key (EF thường tự nhận diện nhưng khai báo rõ cho chắc chắn)
            modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeID);
            modelBuilder.Entity<User>().HasKey(u => u.UserID);
            modelBuilder.Entity<Payroll>().HasKey(p => p.PayrollID);
            modelBuilder.Entity<Attendance>().HasKey(a => a.AttendanceID);

            // 2. Cấu hình quan hệ giữa User và Employee (nếu có navigation property)

            // 2.5 Seed Data: Chèn 5 phòng ban mẫu TRƯỚC khi chèn nhân viên
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentID = 1, DepartmentName = "Board of Directors", HeadcountLimit = 5 },
                new Department { DepartmentID = 2, DepartmentName = "IT & Technology Department", HeadcountLimit = 20 },
                new Department { DepartmentID = 3, DepartmentName = "Human Resources Department (HR)", HeadcountLimit = 15 },
                new Department { DepartmentID = 4, DepartmentName = "Marketing Department", HeadcountLimit = 25 },
                new Department { DepartmentID = 5, DepartmentName = "Accounting Department", HeadcountLimit = 10 }
            );
            // Seed Data cho User (để test chức năng Login)
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Username = "admin", Password = "123", RoleID = 1, EmployeeID = null, IsActive = true },
                new User { UserID = 2, Username = "manager", Password = "abc", RoleID = 2, EmployeeID = null, IsActive = true },
                new User { UserID = 3, Username = "staff", Password = "a1b2", RoleID = 3, EmployeeID = null, IsActive = true }
            );
        }
    }
}