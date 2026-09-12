using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesalePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellingPrice",
                table: "Products",
                newName: "SuggestedRetailPrice");

            migrationBuilder.RenameColumn(
                name: "CostPrice",
                table: "Products",
                newName: "DefaultSellingPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SuggestedRetailPrice",
                table: "Products",
                newName: "SellingPrice");

            migrationBuilder.RenameColumn(
                name: "DefaultSellingPrice",
                table: "Products",
                newName: "CostPrice");
        }
    }
}
