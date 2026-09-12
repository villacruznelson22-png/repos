using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesalePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Provinces_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CityMunicipalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityMunicipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityMunicipalities_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CityMunicipalities_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Barangays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityMunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barangays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barangays_CityMunicipalities_CityMunicipalityId",
                        column: x => x.CityMunicipalityId,
                        principalTable: "CityMunicipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barangays_CityMunicipalityId",
                table: "Barangays",
                column: "CityMunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Barangays_Code",
                table: "Barangays",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityMunicipalities_Code",
                table: "CityMunicipalities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityMunicipalities_ProvinceId",
                table: "CityMunicipalities",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_CityMunicipalities_RegionId",
                table: "CityMunicipalities",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_Code",
                table: "Provinces",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_RegionId",
                table: "Provinces",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Barangays");

            migrationBuilder.DropTable(
                name: "CityMunicipalities");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
