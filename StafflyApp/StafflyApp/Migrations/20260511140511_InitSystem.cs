using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StafflyApp.Migrations
{
    /// <inheritdoc />
    public partial class InitSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeadcountLimit = table.Column<int>(type: "int", nullable: false),
                    CurrentStaffCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

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
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID");
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentID", "CurrentStaffCount", "DepartmentName", "HeadcountLimit" },
                values: new object[,]
                {
                    { 1, 0, "Ban Giám Đốc", 5 },
                    { 2, 0, "Phòng IT & Công Nghệ", 20 },
                    { 3, 0, "Phòng Nhân Sự (HR)", 15 },
                    { 4, 0, "Phòng Marketing", 25 },
                    { 5, 0, "Phòng Kế Toán", 10 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "EmployeeID", "IsActive", "Password", "RoleID", "Username" },
                values: new object[] { 1, 1, true, "123", 1, "admin" });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeID", "Address", "DateOfBirth", "DepartmentID", "Email", "FullName", "Phone", "Status" },
                values: new object[,]
                {
                    { 1, "TP.HCM", null, 1, "kieuvy611@staffly.com", "Lê Nguyễn Kiều Vy", "0901234567", "Active" },
                    { 2, "Hà Nội", null, 1, "tvy1611@staffly.com", "Nguyễn Phạm Thúy Vy", "0901112223", "Active" },
                    { 3, "Đà Nẵng", null, 2, "qthink1006@staffly.com", "Nguyễn Huỳnh Quốc Thịnh", "0903334445", "Active" },
                    { 4, "Cần Thơ", null, 2, "thaouyen@staffly.com", "Hoàng Thị Thảo Uyên", "0905556667", "Active" },
                    { 5, "TP.HCM", null, 3, "taha0302@staffly.com", "Lý Thái Hòa", "0907778889", "Inactive" },
                    { 6, "Bình Dương", null, 1, "bvi0610@staffly.com", "Võ Thị Bảo Vy", "0909990001", "Active" },
                    { 7, "Đồng Nai", null, 2, "vminhtien@staffly.com", "Vương Minh Tiến", "0911223344", "Active" },
                    { 8, "Long An", null, 4, "diva@staffly.com", "Lady Gaga", "0912233445", "Active" },
                    { 9, "Vũng Tàu", null, 3, "jack@staffly.com", "Leonardo Dicaprio", "0913344556", "Active" },
                    { 10, "Tiền Giang", null, 5, "katty@staffly.com", "Katty Perry", "0914455667", "Active" },
                    { 11, "Kiên Giang", null, 1, "thv@staffly.com", "Kim TaeHyung", "0915566778", "Active" },
                    { 12, "An Giang", null, 1, "jk97@staffly.com", "Jeon JungKook", "0916677889", "Active" },
                    { 13, "TP.HCM", null, 2, "yoonjunggo@staffly.com", "Go Yoon Jung", "0917788990", "Active" },
                    { 14, "Bến Tre", null, 1, "xgh@staffly.com", "Xu Guang Han", "0918899001", "Active" },
                    { 15, "TP.HCM", null, 4, "zrn@staffly.com", "Zhang Ruo Nan", "0919900112", "Active" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentID",
                table: "Employees",
                column: "DepartmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
