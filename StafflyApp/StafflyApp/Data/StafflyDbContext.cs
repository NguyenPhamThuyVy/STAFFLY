using Microsoft.EntityFrameworkCore;
using StafflyApp.Models; 

namespace StafflyApp.Data
{
    public class StafflyDbContext : DbContext
    {
        // Trong file StafflyDbContext.cs

        public StafflyDbContext() { } // Thêm dòng này

        public StafflyDbContext(DbContextOptions<StafflyDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Thay MSI\LNKV bằng QTHINK\THINK (tên máy của Thịnh)
                // Thay Catalog=QLNS (nếu có) bằng StafflyDB cho đúng file Snapshot
                optionsBuilder.UseSqlServer(@"Data Source=QTHINK\THINK;Initial Catalog=StafflyDB;Integrated Security=True;TrustServerCertificate=True;");
            }
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Primary Key (EF thường tự nhận diện nhưng khai báo rõ cho chắc chắn)
            modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeID);
            modelBuilder.Entity<User>().HasKey(u => u.UserID);

            // 2. Cấu hình quan hệ giữa User và Employee (nếu có navigation property)

            // 3. Seed Data: Chèn 15 nhân viên mẫu
            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeID = 1, FullName = "Lê Nguyễn Kiều Vy", Email = "kieuvy611@staffly.com", Phone = "0901234567", Address = "TP.HCM", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 2, FullName = "Nguyễn Phạm Thúy Vy", Email = "tvy1611@staffly.com", Phone = "0901112223", Address = "Hà Nội", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 3, FullName = "Nguyễn Huỳnh Quốc Thịnh", Email = "qthink1006@staffly.com", Phone = "0903334445", Address = "Đà Nẵng", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 4, FullName = "Hoàng Thị Thảo Uyên", Email = "thaouyen@staffly.com", Phone = "0905556667", Address = "Cần Thơ", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 5, FullName = "Lý Thái Hòa", Email = "taha0302@staffly.com", Phone = "0907778889", Address = "TP.HCM", Status = "Inactive", DepartmentID = 3 },
                new Employee { EmployeeID = 6, FullName = "Võ Thị Bảo Vy", Email = "bvi0610@staffly.com", Phone = "0909990001", Address = "Bình Dương", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 7, FullName = "Vương Minh Tiến", Email = "vminhtien@staffly.com", Phone = "0911223344", Address = "Đồng Nai", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 8, FullName = "Lady Gaga", Email = "diva@staffly.com", Phone = "0912233445", Address = "Long An", Status = "Active", DepartmentID = 4 },
                new Employee { EmployeeID = 9, FullName = "Leonardo Dicaprio", Email = "jack@staffly.com", Phone = "0913344556", Address = "Vũng Tàu", Status = "Active", DepartmentID = 3 },
                new Employee { EmployeeID = 10, FullName = "Katty Perry", Email = "katty@staffly.com", Phone = "0914455667", Address = "Tiền Giang", Status = "Active", DepartmentID = 5 },
                new Employee { EmployeeID = 11, FullName = "Kim TaeHyung", Email = "thv@staffly.com", Phone = "0915566778", Address = "Kiên Giang", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 12, FullName = "Jeon JungKook", Email = "jk97@staffly.com", Phone = "0916677889", Address = "An Giang", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 13, FullName = "Go Yoon Jung", Email = "yoonjunggo@staffly.com", Phone = "0917788990", Address = "TP.HCM", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 14, FullName = "Xu Guang Han", Email = "xgh@staffly.com", Phone = "0918899001", Address = "Bến Tre", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 15, FullName = "Zhang Ruo Nan", Email = "zrn@staffly.com", Phone = "0919900112", Address = "TP.HCM", Status = "Active", DepartmentID = 4 }
            );

            // Seed Data cho User (để test chức năng Login)
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Username = "admin", Password = "123", RoleID = 1, EmployeeID = 1, IsActive = true }
            );
        }
    }
}