using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StafflyApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeID", "Address", "DateOfBirth", "DepartmentID", "Email", "FullName", "Phone", "Status" },
                values: new object[,]
                {
                    { 1, "TP.HCM", null, 1, "kieuvy611@staffly.com", "Lê Nguyễn Kiều Vy", "0901234567", "Active" },
                    { 2, "Hà Nội", null, 1, "a.tv@staffly.com", "Trần Văn A", "0901112223", "Active" },
                    { 3, "Đà Nẵng", null, 2, "b.lt@staffly.com", "Lê Thị B", "0903334445", "Active" },
                    { 4, "Cần Thơ", null, 2, "c.pv@staffly.com", "Phạm Văn C", "0905556667", "Active" },
                    { 5, "TP.HCM", null, 3, "d.ht@staffly.com", "Hoàng Thị D", "0907778889", "Inactive" },
                    { 6, "Bình Dương", null, 1, "e.dv@staffly.com", "Đỗ Văn E", "0909990001", "Active" },
                    { 7, "Đồng Nai", null, 2, "f.bt@staffly.com", "Bùi Thị F", "0911223344", "Active" },
                    { 8, "Long An", null, 4, "g.vv@staffly.com", "Vũ Văn G", "0912233445", "Active" },
                    { 9, "Vũng Tàu", null, 3, "h.pt@staffly.com", "Phan Thị H", "0913344556", "Active" },
                    { 10, "Tiền Giang", null, 5, "i.lv@staffly.com", "Lý Văn I", "0914455667", "Active" },
                    { 11, "Kiên Giang", null, 1, "k.tt@staffly.com", "Trịnh Thị K", "0915566778", "Active" },
                    { 12, "An Giang", null, 1, "l.lv@staffly.com", "Lưu Văn L", "0916677889", "Active" },
                    { 13, "TP.HCM", null, 2, "m.mt@staffly.com", "Mai Thị M", "0917788990", "Active" },
                    { 14, "Bến Tre", null, 1, "n.dv@staffly.com", "Đào Văn N", "0918899001", "Active" },
                    { 15, "TP.HCM", null, 4, "o.dt@staffly.com", "Đặng Thị O", "0919900112", "Active" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "EmployeeID", "IsActive", "Password", "RoleID", "Username" },
                values: new object[] { 1, 1, true, "123", 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
