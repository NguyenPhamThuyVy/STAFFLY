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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeID);
            modelBuilder.Entity<User>().HasKey(u => u.UserID);

            // CHỈ GIỮ LẠI SEED DATA PHÒNG BAN (Để có dữ liệu chọn lúc Add Nhân viên)
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentID = 1, DepartmentName = "Ban Giám Đốc", HeadcountLimit = 5 },
                new Department { DepartmentID = 2, DepartmentName = "Phòng IT & Công Nghệ", HeadcountLimit = 20 },
                new Department { DepartmentID = 3, DepartmentName = "Phòng Nhân Sự (HR)", HeadcountLimit = 15 },
                new Department { DepartmentID = 4, DepartmentName = "Phòng Marketing", HeadcountLimit = 25 },
                new Department { DepartmentID = 5, DepartmentName = "Phòng Kế Toán", HeadcountLimit = 10 }
            );

            // GIỮ LẠI SEED DATA TÀI KHOẢN (Để đăng nhập nếu có)
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Username = "admin", Password = "123", RoleID = 1, EmployeeID = 1, IsActive = true },
                new User { UserID = 2, Username = "manager", Password = "123", RoleID = 2, EmployeeID = 2, IsActive = true },
                new User { UserID = 3, Username = "staff", Password = "123", RoleID = 3, EmployeeID = 3, IsActive = true }
            );

            // (ĐÃ XÓA TOÀN BỘ ĐOẠN modelBuilder.Entity<Employee>().HasData(...) Ở ĐÂY)
        }
    }
    }
