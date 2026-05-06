using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StafflyApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsersAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 2,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "tvy1611@staffly.com", "Nguyễn Phạm Thúy Vy" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 3,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "qthink1006@staffly.com", "Nguyễn Huỳnh Quốc Thịnh" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 4,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "thaouyen@staffly.com", "Hoàng Thị Thảo Uyên" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 5,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "taha0302@staffly.com", "Lý Thái Hòa" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 6,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "bvi0610@staffly.com", "Võ Thị Bảo Vy" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 7,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "vminhtien@staffly.com", "Vương Minh Tiến" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 8,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "diva@staffly.com", "Lady Gaga" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 9,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "jack@staffly.com", "Leonardo Dicaprio" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 10,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "katty@staffly.com", "Katty Perry" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 11,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "thv@staffly.com", "Kim TaeHyung" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 12,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "jk97@staffly.com", "Jeon JungKook" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 13,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "yoonjunggo@staffly.com", "Go Yoon Jung" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 14,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "xgh@staffly.com", "Xu Guang Han" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 15,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "zrn@staffly.com", "Zhang Ruo Nan" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "EmployeeID", "IsActive", "Password", "RoleID", "Username" },
                values: new object[,]
                {
                    { 2, 2, true, "123", 2, "manager" },
                    { 3, 3, true, "123", 3, "staff" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 2,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "a.tv@staffly.com", "Trần Văn A" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 3,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "b.lt@staffly.com", "Lê Thị B" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 4,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "c.pv@staffly.com", "Phạm Văn C" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 5,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "d.ht@staffly.com", "Hoàng Thị D" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 6,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "e.dv@staffly.com", "Đỗ Văn E" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 7,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "f.bt@staffly.com", "Bùi Thị F" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 8,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "g.vv@staffly.com", "Vũ Văn G" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 9,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "h.pt@staffly.com", "Phan Thị H" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 10,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "i.lv@staffly.com", "Lý Văn I" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 11,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "k.tt@staffly.com", "Trịnh Thị K" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 12,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "l.lv@staffly.com", "Lưu Văn L" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 13,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "m.mt@staffly.com", "Mai Thị M" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 14,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "n.dv@staffly.com", "Đào Văn N" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeID",
                keyValue: 15,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "o.dt@staffly.com", "Đặng Thị O" });
        }
    }
}
