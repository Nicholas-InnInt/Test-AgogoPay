using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_PayMents_Rate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MoMoRate",
                table: "PayMents",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VittelPayRate",
                table: "PayMents",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ZaloRate",
                table: "PayMents",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "NsPayBackgroundJobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRestart",
                table: "NsPayBackgroundJobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoMoRate",
                table: "PayMents");

            migrationBuilder.DropColumn(
                name: "VittelPayRate",
                table: "PayMents");

            migrationBuilder.DropColumn(
                name: "ZaloRate",
                table: "PayMents");

            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "NsPayBackgroundJobs");

            migrationBuilder.DropColumn(
                name: "IsRestart",
                table: "NsPayBackgroundJobs");
        }
    }
}
