using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_WithdrawalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WithdrawalOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    PlatformCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WithdrawNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    OrderMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BenAccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BenAccountNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BenBankName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NotifyUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    NotifyStatus = table.Column<int>(type: "int", nullable: false),
                    NotifyNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationUnixTime = table.Column<long>(type: "bigint", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalOrders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WithdrawalOrders");
        }
    }
}
