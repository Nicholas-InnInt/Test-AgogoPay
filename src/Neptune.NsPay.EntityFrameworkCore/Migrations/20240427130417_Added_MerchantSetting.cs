using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_MerchantSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MerchantSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    NsPayTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoginIpAddress = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    BankApi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VietcomApi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BankNotify = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    BankNotifyText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TelegramNotifyBotId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TelegramNotifyChatId = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OpenRiskWithdrawal = table.Column<bool>(type: "bit", nullable: false),
                    PlatformUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlatformUserName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlatformPassWord = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlatformLimitMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_MerchantSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantSettings");
        }
    }
}
