using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_PayMent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayMents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CompanyType = table.Column<int>(type: "int", nullable: false),
                    Gateway = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CompanyKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanySecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusinessType = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QrCode = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PassWord = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoMoCheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoMoPHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LimitMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceLimitMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UseMoMo = table.Column<bool>(type: "bit", nullable: false),
                    DispenseType = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayMents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayMents");
        }
    }
}
