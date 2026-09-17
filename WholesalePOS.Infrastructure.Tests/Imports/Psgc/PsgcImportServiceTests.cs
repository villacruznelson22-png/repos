using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Imports.Psgc;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests.Imports.Psgc;



public class PsgcImportServiceTests
{
    [Fact]
    [TestDatabase]
    public async Task ImportAsync_ShouldPersistPsgcHierarchy()
    {
        // Arrange
        var factory = new TestDbContextFactory();

        await using var context =
            factory.Create();

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
    [TestDatabase]
    public async Task ImportAsync_ShouldRollback_WhenImportFails()
    {
        // Arrange
        var factory = new TestDbContextFactory();

        await using var context =
            factory.Create();

        var importer =
            new PsgcImporter();

        var service =
            new PsgcImportService(context);

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
        // in the test database.
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