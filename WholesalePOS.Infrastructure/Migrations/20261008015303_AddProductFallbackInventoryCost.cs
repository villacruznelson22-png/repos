using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesalePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFallbackInventoryCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FallbackInventoryCost",
                table: "Products",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FallbackInventoryCost",
                table: "Products");
        }
    }
}
