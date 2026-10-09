using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesalePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleVoidAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "Sales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoidedByUserId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_VoidedByUserId",
                table: "Sales",
                column: "VoidedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Users_VoidedByUserId",
                table: "Sales",
                column: "VoidedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Users_VoidedByUserId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_VoidedByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "VoidedByUserId",
                table: "Sales");
        }
    }
}
