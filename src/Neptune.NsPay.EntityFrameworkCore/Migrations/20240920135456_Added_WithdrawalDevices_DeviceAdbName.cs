using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_WithdrawalDevices_DeviceAdbName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceAdbName",
                table: "WithdrawalDevices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderBankRemark",
                table: "MerchantSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceAdbName",
                table: "WithdrawalDevices");

            migrationBuilder.AlterColumn<string>(
                name: "OrderBankRemark",
                table: "MerchantSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
