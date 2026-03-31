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
                new Employee { EmployeeID = 2, FullName = "Trần Văn A", Email = "a.tv@staffly.com", Phone = "0901112223", Address = "Hà Nội", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 3, FullName = "Lê Thị B", Email = "b.lt@staffly.com", Phone = "0903334445", Address = "Đà Nẵng", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 4, FullName = "Phạm Văn C", Email = "c.pv@staffly.com", Phone = "0905556667", Address = "Cần Thơ", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 5, FullName = "Hoàng Thị D", Email = "d.ht@staffly.com", Phone = "0907778889", Address = "TP.HCM", Status = "Inactive", DepartmentID = 3 },
                new Employee { EmployeeID = 6, FullName = "Đỗ Văn E", Email = "e.dv@staffly.com", Phone = "0909990001", Address = "Bình Dương", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 7, FullName = "Bùi Thị F", Email = "f.bt@staffly.com", Phone = "0911223344", Address = "Đồng Nai", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 8, FullName = "Vũ Văn G", Email = "g.vv@staffly.com", Phone = "0912233445", Address = "Long An", Status = "Active", DepartmentID = 4 },
                new Employee { EmployeeID = 9, FullName = "Phan Thị H", Email = "h.pt@staffly.com", Phone = "0913344556", Address = "Vũng Tàu", Status = "Active", DepartmentID = 3 },
                new Employee { EmployeeID = 10, FullName = "Lý Văn I", Email = "i.lv@staffly.com", Phone = "0914455667", Address = "Tiền Giang", Status = "Active", DepartmentID = 5 },
                new Employee { EmployeeID = 11, FullName = "Trịnh Thị K", Email = "k.tt@staffly.com", Phone = "0915566778", Address = "Kiên Giang", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 12, FullName = "Lưu Văn L", Email = "l.lv@staffly.com", Phone = "0916677889", Address = "An Giang", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 13, FullName = "Mai Thị M", Email = "m.mt@staffly.com", Phone = "0917788990", Address = "TP.HCM", Status = "Active", DepartmentID = 2 },
                new Employee { EmployeeID = 14, FullName = "Đào Văn N", Email = "n.dv@staffly.com", Phone = "0918899001", Address = "Bến Tre", Status = "Active", DepartmentID = 1 },
                new Employee { EmployeeID = 15, FullName = "Đặng Thị O", Email = "o.dt@staffly.com", Phone = "0919900112", Address = "TP.HCM", Status = "Active", DepartmentID = 4 }
            );

            // Seed Data cho User (để test chức năng Login)
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Username = "admin", Password = "123", RoleID = 1, EmployeeID = 1, IsActive = true }
            );
        }
    }
}