using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Infrastructure.Imports.Psgc;

public sealed class PsgcImporter
{
    public IReadOnlyList<Region> Import(
        IReadOnlyList<PsgcRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        if (rows.Count == 0)
        {
            return [];
        }

        var rowsByCode = CreateRowLookup(rows);

        var regions = new Dictionary<string, Region>();
        var provinces = new Dictionary<string, Province>();
        var citiesMunicipalities =
            new Dictionary<string, CityMunicipality>();

        // ==================================================
        // PASS 1: REGIONS
        // ==================================================

        foreach (var row in rows)
        {
            if (row.GeographicLevel != "Reg")
            {
                continue;
            }

            var region = new Region(
                row.Code,
                row.Name);

            regions.Add(
                row.Code,
                region);
        }

        // ==================================================
        // PASS 2: PROVINCES
        // ==================================================

        foreach (var row in rows)
        {
            if (row.GeographicLevel != "Prov")
            {
                continue;
            }

            var regionCode =
                PsgcCodeParser.GetRegionCode(row.Code);

            if (!regions.TryGetValue(
                    regionCode,
                    out var region))
            {
                throw new InvalidOperationException(
                    $"Region '{regionCode}' was not found " +
                    $"for province '{row.Name}' " +
                    $"({row.Code}).");
            }

            var province = new Province(
                region.Id,
                row.Code,
                row.Name);

            region.Provinces.Add(province);

            provinces.Add(
                row.Code,
                province);
        }

        // ==================================================
        // PASS 3: CITIES / MUNICIPALITIES
        // ==================================================

        foreach (var row in rows)
        {
            if (row.GeographicLevel is not ("City" or "Mun"))
            {
                continue;
            }

            var regionCode =
                PsgcCodeParser.GetRegionCode(row.Code);

            if (!regions.TryGetValue(
                    regionCode,
                    out var region))
            {
                throw new InvalidOperationException(
                    $"Region '{regionCode}' was not found " +
                    $"for '{row.Name}' " +
                    $"({row.Code}).");
            }

            Province? province = null;

            var provinceCode =
                PsgcCodeParser.GetProvinceCode(row.Code);

            provinces.TryGetValue(
                provinceCode,
                out province);

            if (province is null &&
                !CanExistWithoutProvince(
                    row,
                    regionCode,
                    provinceCode))
            {
                throw new InvalidOperationException(
                    $"Province '{provinceCode}' was not found " +
                    $"for '{row.Name}' " +
                    $"({row.Code}).");
            }

            var type =
                row.GeographicLevel == "City"
                    ? CityMunicipalityType.City
                    : CityMunicipalityType.Municipality;

            var cityMunicipality =
                new CityMunicipality(
                    region.Id,
                    province?.Id,
                    row.Code,
                    row.Name,
                    type);

            region.CityMunicipalities.Add(
                cityMunicipality);

            province?.CityMunicipalities.Add(
                cityMunicipality);

            citiesMunicipalities.Add(
                row.Code,
                cityMunicipality);
        }

        // ==================================================
        // PASS 4: BARANGAYS
        // ==================================================

        foreach (var row in rows)
        {
            if (row.GeographicLevel != "Bgy")
            {
                continue;
            }

            var cityMunicipalityCode =
                ResolveCityMunicipalityCode(
                    row,
                    rowsByCode);

            if (!citiesMunicipalities.TryGetValue(
                    cityMunicipalityCode,
                    out var cityMunicipality))
            {
                throw new InvalidOperationException(
                    $"City/Municipality " +
                    $"'{cityMunicipalityCode}' was not found " +
                    $"for barangay '{row.Name}' " +
                    $"({row.Code}).");
            }

            var barangay = new Barangay(
                cityMunicipality.Id,
                row.Code,
                row.Name);

            cityMunicipality.Barangays.Add(
                barangay);
        }

        return regions.Values.ToList();
    }

    // ======================================================
    // CITY / MUNICIPALITY RESOLUTION
    // ======================================================

    private static string ResolveCityMunicipalityCode(
        PsgcRow barangay,
        IReadOnlyDictionary<string, PsgcRow> rowsByCode)
    {
        var currentCode =
            PsgcCodeParser.GetCityMunicipalityCode(
                barangay.Code);

        var visitedCodes =
            new HashSet<string>();

        while (true)
        {
            if (!visitedCodes.Add(currentCode))
            {
                throw new InvalidOperationException(
                    $"Circular PSGC hierarchy detected " +
                    $"while resolving barangay " +
                    $"'{barangay.Name}' ({barangay.Code}).");
            }

            if (!rowsByCode.TryGetValue(
                    currentCode,
                    out var currentRow))
            {
                throw new InvalidOperationException(
                    $"Parent PSGC '{currentCode}' was not found " +
                    $"for barangay '{barangay.Name}' " +
                    $"({barangay.Code}).");
            }

            // Normal case:
            //
            // Barangay
            //      ↓
            // City / Municipality
            //
            if (currentRow.GeographicLevel is "City" or "Mun")
            {
                return currentRow.Code;
            }

            // Manila-style case:
            //
            // Barangay
            //      ↓
            // SubMun
            //      ↓
            // City
            //
            if (currentRow.GeographicLevel == "SubMun")
            {
                currentCode =
                    PsgcCodeParser
                        .GetSubMunicipalityParentCode(
                            currentRow.Code);

                continue;
            }

            throw new InvalidOperationException(
                $"Unsupported geographic level " +
                $"'{currentRow.GeographicLevel}' " +
                $"encountered while resolving barangay " +
                $"'{barangay.Name}' ({barangay.Code}).");
        }
    }

    // ======================================================
    // PROVINCE RESOLUTION
    // ======================================================

    private static bool CanExistWithoutProvince(
        PsgcRow row,
        string regionCode,
        string provinceCode)
    {
        // NCR has no provinces.
        if (regionCode == "1300000000")
        {
            return true;
        }

        // Independent cities can exist directly
        // under a region.
        if (row.GeographicLevel == "City")
        {
            return true;
        }

        // BARMM Special Geographic Area.
        if (provinceCode == "1999900000")
        {
            return true;
        }

        return false;
    }

    // ======================================================
    // LOOKUP
    // ======================================================

    private static Dictionary<string, PsgcRow>
        CreateRowLookup(
            IReadOnlyList<PsgcRow> rows)
    {
        var rowsByCode =
            new Dictionary<string, PsgcRow>();

        foreach (var row in rows)
        {
            if (!rowsByCode.TryAdd(
                    row.Code,
                    row))
            {
                throw new InvalidOperationException(
                    $"Duplicate PSGC code detected: " +
                    $"'{row.Code}'.");
            }
        }

        return rowsByCode;
    }
}