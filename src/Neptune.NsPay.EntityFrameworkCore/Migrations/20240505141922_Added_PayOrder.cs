using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_PayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    OrderMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationUnixTime = table.Column<long>(type: "bigint", nullable: false),
                    OrderMark = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlatformCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PayMentId = table.Column<int>(type: "int", nullable: false),
                    ScCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScSeri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NotifyUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScoreStatus = table.Column<int>(type: "int", nullable: false),
                    PayType = table.Column<int>(type: "int", nullable: false),
                    ScoreNumber = table.Column<int>(type: "int", nullable: false),
                    TradeMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ErrorMsg = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    PaymentChannel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayOrders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayOrders");
        }
    }
}
