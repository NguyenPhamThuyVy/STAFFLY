using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StafflyApp.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationUserSecurityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Chỉ tạo 3 cột còn thiếu này thôi
            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultPassword",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "employee@staffly.com");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.Now);

            // 2. Ép dữ liệu seed data lên True hết (Bao gồm cả cột IsActive có sẵn)
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "IsActive", "IsDefaultPassword", "Email", "CreatedAt" },
                values: new object[] { true, false, "admin@staffly.com", DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2,
                columns: new[] { "IsActive", "IsDefaultPassword", "Email", "CreatedAt" },
                values: new object[] { true, false, "manager@staffly.com", DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 3,
                columns: new[] { "IsActive", "IsDefaultPassword", "Email", "CreatedAt" },
                values: new object[] { true, false, "staff@staffly.com", DateTime.Now });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsDefaultPassword", table: "Users");
            migrationBuilder.DropColumn(name: "Email", table: "Users");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Users");
        }
    }
}