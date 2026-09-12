using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Imports.Psgc;

public sealed class PsgcImportService
{
    private readonly WholesalePosDbContext _context;

    public PsgcImportService(
        WholesalePosDbContext context)
    {
        _context = context;
    }

    public async Task ImportAsync(
        IReadOnlyList<Region> regions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(regions);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // ============================================
            // Add Regions
            // ============================================

            await _context.Regions.AddRangeAsync(
                regions,
                cancellationToken);

            // ============================================
            // Add Provinces
            // ============================================

            var provinces =
                regions
                    .SelectMany(x => x.Provinces)
                    .ToList();

            await _context.Provinces.AddRangeAsync(
                provinces,
                cancellationToken);

            // ============================================
            // Add Cities / Municipalities
            // ============================================

            var cityMunicipalities =
                regions
                    .SelectMany(x => x.CityMunicipalities)
                    .ToList();

            await _context.CityMunicipalities.AddRangeAsync(
                cityMunicipalities,
                cancellationToken);

            // ============================================
            // Add Barangays
            // ============================================

            var barangays =
                cityMunicipalities
                    .SelectMany(x => x.Barangays)
                    .ToList();

            await _context.Barangays.AddRangeAsync(
                barangays,
                cancellationToken);

            // ============================================
            // Save everything
            // ============================================

            await _context.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}