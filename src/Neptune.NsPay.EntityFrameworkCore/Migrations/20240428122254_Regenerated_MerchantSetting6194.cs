using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Regenerated_MerchantSetting6194 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankApi",
                table: "MerchantSettings");

            migrationBuilder.DropColumn(
                name: "VietcomApi",
                table: "MerchantSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankApi",
                table: "MerchantSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietcomApi",
                table: "MerchantSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
