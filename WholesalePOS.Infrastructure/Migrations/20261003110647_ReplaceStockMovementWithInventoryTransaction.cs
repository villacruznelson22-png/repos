using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesalePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStockMovementWithInventoryTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the new inventory transaction ledger first.
            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    ProductId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Type = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Direction = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Quantity = table.Column<decimal>(
                        type: "decimal(18,3)",
                        precision: 18,
                        scale: 3,
                        nullable: true),

                    UnitCost = table.Column<decimal>(
                        type: "decimal(19,6)",
                        precision: 19,
                        scale: 6,
                        nullable: true),

                    TotalCost = table.Column<decimal>(
                        type: "decimal(19,6)",
                        precision: 19,
                        scale: 6,
                        nullable: false),

                    OccurredAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    ReferenceType = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: true),

                    ReferenceId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_InventoryTransactions",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Preserve existing StockMovement history.
            //
            // Old StockMovementType:
            // 1 = Purchase
            // 2 = Sale
            // 3 = CustomerReturn
            // 4 = Damage
            // 5 = Adjustment
            //
            // New InventoryTransactionType:
            // 1 = OpeningBalance
            // 2 = Purchase
            // 3 = Sale
            // 4 = CustomerReturn
            // 5 = Damage
            // 6 = Adjustment
            // 7 = CostCorrection
            //
            // Therefore the old enum values must be explicitly remapped.
            //
            // Old records do not contain historical cost information,
            // so UnitCost remains NULL and TotalCost is 0.
            migrationBuilder.Sql("""
                INSERT INTO InventoryTransactions
                (
                    Id,
                    ProductId,
                    Type,
                    Direction,
                    Quantity,
                    UnitCost,
                    TotalCost,
                    OccurredAt,
                    ReferenceType,
                    ReferenceId
                )
                SELECT
                    Id,
                    ProductId,
                    CASE Type
                        WHEN 1 THEN 2 -- Purchase
                        WHEN 2 THEN 3 -- Sale
                        WHEN 3 THEN 4 -- CustomerReturn
                        WHEN 4 THEN 5 -- Damage
                        WHEN 5 THEN 6 -- Adjustment
                    END,
                    Direction,
                    Quantity,
                    NULL,
                    0,
                    OccurredAt,
                    NULL,
                    NULL
                FROM StockMovements;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId_OccurredAt",
                table: "InventoryTransactions",
                columns: new[]
                {
                    "ProductId",
                    "OccurredAt"
                });

            // Only after the old data has been copied do we remove
            // the obsolete StockMovements table.
            migrationBuilder.DropTable(
                name: "StockMovements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the legacy StockMovements table.
            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Direction = table.Column<int>(
                        type: "int",
                        nullable: false),

                    OccurredAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    ProductId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Quantity = table.Column<decimal>(
                        type: "decimal(18,3)",
                        precision: 18,
                        scale: 3,
                        nullable: false),

                    Type = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_StockMovements",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Restore only transaction types that existed in the
            // original StockMovement model.
            //
            // New-only transactions such as OpeningBalance and
            // CostCorrection cannot be represented by StockMovement.
            migrationBuilder.Sql("""
                INSERT INTO StockMovements
                (
                    Id,
                    Direction,
                    OccurredAt,
                    ProductId,
                    Quantity,
                    Type
                )
                SELECT
                    Id,
                    Direction,
                    OccurredAt,
                    ProductId,
                    Quantity,
                    CASE Type
                        WHEN 2 THEN 1 -- Purchase
                        WHEN 3 THEN 2 -- Sale
                        WHEN 4 THEN 3 -- CustomerReturn
                        WHEN 5 THEN 4 -- Damage
                        WHEN 6 THEN 5 -- Adjustment
                    END
                FROM InventoryTransactions
                WHERE Type IN (2, 3, 4, 5, 6);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");
        }
    }
}