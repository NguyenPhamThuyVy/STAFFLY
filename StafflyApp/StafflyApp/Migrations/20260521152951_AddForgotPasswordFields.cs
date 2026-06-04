using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StafflyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddForgotPasswordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResetRequested",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TempPasswordPlain",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "IsResetRequested", "TempPasswordPlain" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2,
                columns: new[] { "IsResetRequested", "TempPasswordPlain" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 3,
                columns: new[] { "IsResetRequested", "TempPasswordPlain" },
                values: new object[] { false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResetRequested",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TempPasswordPlain",
                table: "Users");
        }
    }
}
