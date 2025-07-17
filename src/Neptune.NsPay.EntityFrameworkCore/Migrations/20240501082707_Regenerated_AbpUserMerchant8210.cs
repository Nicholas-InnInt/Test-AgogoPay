using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Regenerated_AbpUserMerchant8210 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Merchants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "AbpUserMerchants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_UserId",
                table: "Merchants",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Merchants_AbpUsers_UserId",
                table: "Merchants",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Merchants_AbpUsers_UserId",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_UserId",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Merchants");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "AbpUserMerchants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
