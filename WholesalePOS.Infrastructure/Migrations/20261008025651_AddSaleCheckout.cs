using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesalePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutIdempotencyKey",
                table: "Sales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Sales",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "InventoryBalances",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CheckoutIdempotencyKey",
                table: "Sales",
                column: "CheckoutIdempotencyKey",
                unique: true,
                filter: "[CheckoutIdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_CheckoutIdempotencyKey",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CheckoutIdempotencyKey",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "InventoryBalances");
        }
    }
}
