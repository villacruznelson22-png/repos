using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Imports.Psgc;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests.Imports.Psgc;

public class PsgcImportServiceTests
{
    [Fact]
    public async Task ImportAsync_ShouldPersistPsgcHierarchy()
    {
        // Arrange
        var options =
            new DbContextOptionsBuilder<WholesalePosDbContext>()
                .UseSqlServer(
                    "Server=.\\SQLEXPRESS;Database=WholesalePOSDb;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

        await using var context =
            new WholesalePosDbContext(options);

        var reader =
            new PsgcExcelReader();

        var importer =
            new PsgcImporter();

        var service =
            new PsgcImportService(context);

        var rows =
            reader.Read(
                   @"C:\Users\Nelson Villacruz\Downloads\PSGC-2Q-2026-Publication-Datafile.xlsx");


        var regions =
            importer.Import(rows);

        // Act
        await service.ImportAsync(regions);

        // Assert
        Assert.Equal(
            18,
            await context.Regions.CountAsync());

        Assert.Equal(
            82,
            await context.Provinces.CountAsync());

        Assert.Equal(
            1642,
            await context.CityMunicipalities.CountAsync());

        Assert.Equal(
            42010,
            await context.Barangays.CountAsync());
    }

    [Fact]
    public async Task ImportAsync_ShouldRollback_WhenImportFails()
    {
        // Arrange
        var options =
            new DbContextOptionsBuilder<WholesalePosDbContext>()
                .UseSqlServer(
                     "Server=.\\SQLEXPRESS;Database=WholesalePOSDb;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

        await using var context =
            new WholesalePosDbContext(options);

        var importer =
            new PsgcImporter();

        var service =
            new PsgcImportService(context);

        // Use a code that is extremely unlikely
        // to exist in the real PSGC data.
        const string testRegionCode =
            "TEST-ROLLBACK-REGION";

        // Clean up ONLY our test data if a previous
        // failed test left anything behind.
        var existingTestRegion =
            await context.Regions
                .SingleOrDefaultAsync(
                    x => x.Code == testRegionCode);

        if (existingTestRegion is not null)
        {
            context.Regions.Remove(
                existingTestRegion);

            await context.SaveChangesAsync();
        }

        // Create a region that already exists
        // in the database.
        var existingRegion =
            new Region(
                testRegionCode,
                "Existing Test Region");

        context.Regions.Add(existingRegion);

        await context.SaveChangesAsync();

        // Create an import containing the SAME region code.
        var rows = new[]
        {
        new PsgcRow(
            testRegionCode,
            "Imported Test Region",
            "Reg",
            "")
    };

        var regions =
            importer.Import(rows);

        // Act
        await Assert.ThrowsAnyAsync<Exception>(
            () =>
                service.ImportAsync(regions));

        // Assert
        // The original record must still exist.
        var region =
            await context.Regions
                .SingleAsync(
                    x => x.Code == testRegionCode);

        Assert.Equal(
            "Existing Test Region",
            region.Name);

        // Cleanup ONLY our test record.
        context.Regions.Remove(region);

        await context.SaveChangesAsync();
    }
}