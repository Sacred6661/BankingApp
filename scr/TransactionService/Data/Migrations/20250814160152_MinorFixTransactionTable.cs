using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionService.Data.Migrations
{
    /// <inheritdoc />
    public partial class MinorFixTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_TransactionTypes_TransactionStatusId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionStatusId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "TransactionStatus",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "TransactionStatusId",
                keyValue: 2,
                column: "Name",
                value: "Accepted");

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "TransactionStatusId",
                keyValue: 3,
                column: "Name",
                value: "Rejected");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "Transactions");

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "TransactionStatusId",
                keyValue: 2,
                column: "Name",
                value: "Completed");

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "TransactionStatusId",
                keyValue: 3,
                column: "Name",
                value: "Failed");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionStatusId",
                table: "Transactions",
                column: "TransactionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_TransactionTypes_TransactionStatusId",
                table: "Transactions",
                column: "TransactionStatusId",
                principalTable: "TransactionTypes",
                principalColumn: "TransactionTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
