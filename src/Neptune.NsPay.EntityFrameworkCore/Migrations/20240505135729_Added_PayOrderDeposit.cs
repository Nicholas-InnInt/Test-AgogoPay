using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Added_PayOrderDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayOrderDeposits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefNo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BankOrderId = table.Column<long>(type: "bigint", nullable: false),
                    PayType = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditBank = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreditAcctNo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreditAcctName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DebitBank = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DebitAcctNo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DebitAcctName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TransactionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationUnixTime = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RejectRemark = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PayMentId = table.Column<int>(type: "int", nullable: false),
                    OperateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayOrderDeposits", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayOrderDeposits");
        }
    }
}
