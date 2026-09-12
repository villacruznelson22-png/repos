using WholesalePOS.Domain.Enums;
using WholesalePOS.Infrastructure.Imports.Psgc;

namespace WholesalePOS.Infrastructure.Tests.Imports.Psgc;

public class PsgcImporterTests
{
    [Fact]
    public void Import_ShouldBuildRizalTaytayHierarchy()
    {
        // Arrange
        var rows = new[]
        {
            new PsgcRow(
                "0400000000",
                "Region IV-A (CALABARZON)",
                "Reg",
                "040000000"),

            new PsgcRow(
                "0405800000",
                "Rizal",
                "Prov",
                "045800000"),

            new PsgcRow(
                "0405813000",
                "Taytay",
                "Mun",
                "045813000"),

            new PsgcRow(
                "0405813001",
                "Dolores",
                "Bgy",
                "045813001")
        };

        var importer = new PsgcImporter();

        // Act
        var regions = importer.Import(rows);

        // Assert
        Assert.Single(regions);

        var region = regions.Single();

        Assert.Equal(
            "Region IV-A (CALABARZON)",
            region.Name);

        Assert.Single(region.Provinces);

        var province = region.Provinces.Single();

        Assert.Equal(
            "Rizal",
            province.Name);

        Assert.Equal(
            region.Id,
            province.RegionId);

        Assert.Single(
            province.CityMunicipalities);

        var municipality =
            province.CityMunicipalities.Single();

        Assert.Equal(
            "Taytay",
            municipality.Name);

        Assert.Equal(
            CityMunicipalityType.Municipality,
            municipality.Type);

        Assert.Equal(
            region.Id,
            municipality.RegionId);

        Assert.Equal(
            province.Id,
            municipality.ProvinceId);

        Assert.Single(
            municipality.Barangays);

        var barangay =
            municipality.Barangays.Single();

        Assert.Equal(
            "Dolores",
            barangay.Name);

        Assert.Equal(
            municipality.Id,
            barangay.CityMunicipalityId);
    }


    [Fact]
    public void Import_ShouldBuildNcrQuezonCityHierarchy()
    {
        // Arrange
        var rows = new[]
        {
            new PsgcRow(
                "1300000000",
                "National Capital Region (NCR)",
                "Reg",
                "130000000"),

            new PsgcRow(
                "1381300000",
                "Quezon City",
                "City",
                "137404000"),

            new PsgcRow(
                "1381300001",
                "Alicia",
                "Bgy",
                "137404001")
        };

        var importer = new PsgcImporter();

        // Act
        var regions = importer.Import(rows);

        // Assert
        Assert.Single(regions);

        var region = regions.Single();

        Assert.Equal(
            "National Capital Region (NCR)",
            region.Name);

        Assert.Empty(
            region.Provinces);

        Assert.Single(
            region.CityMunicipalities);

        var city =
            region.CityMunicipalities.Single();

        Assert.Equal(
            "Quezon City",
            city.Name);

        Assert.Equal(
            CityMunicipalityType.City,
            city.Type);

        Assert.Equal(
            region.Id,
            city.RegionId);

        Assert.Null(
            city.ProvinceId);

        Assert.Single(
            city.Barangays);

        var barangay =
            city.Barangays.Single();

        Assert.Equal(
            "Alicia",
            barangay.Name);

        Assert.Equal(
            city.Id,
            barangay.CityMunicipalityId);
    }


    [Fact]
    public void Import_ShouldAllowCityWithoutProvince()
    {
        // Arrange
        var rows = new[]
        {
            new PsgcRow(
                "1400000000",
                "Cordillera Administrative Region (CAR)",
                "Reg",
                "140000000"),

            new PsgcRow(
                "1430300000",
                "City of Baguio",
                "City",
                "141101000")
        };

        var importer = new PsgcImporter();

        // Act
        var regions = importer.Import(rows);

        // Assert
        Assert.Single(regions);

        var region =
            regions.Single();

        Assert.Equal(
            "Cordillera Administrative Region (CAR)",
            region.Name);

        Assert.Empty(
            region.Provinces);

        Assert.Single(
            region.CityMunicipalities);

        var baguio =
            region.CityMunicipalities.Single();

        Assert.Equal(
            "City of Baguio",
            baguio.Name);

        Assert.Equal(
            CityMunicipalityType.City,
            baguio.Type);

        Assert.Equal(
            region.Id,
            baguio.RegionId);

        Assert.Null(
            baguio.ProvinceId);
    }


    [Fact]
    public void Import_ShouldAllowPaterosWithoutProvince()
    {
        // Arrange
        var rows = new[]
        {
            new PsgcRow(
                "1300000000",
                "National Capital Region (NCR)",
                "Reg",
                "130000000"),

            new PsgcRow(
                "1381701000",
                "Pateros",
                "Mun",
                "137604000")
        };

        var importer = new PsgcImporter();

        // Act
        var regions = importer.Import(rows);

        // Assert
        Assert.Single(regions);

        var region =
            regions.Single();

        Assert.Equal(
            "National Capital Region (NCR)",
            region.Name);

        Assert.Empty(
            region.Provinces);

        Assert.Single(
            region.CityMunicipalities);

        var pateros =
            region.CityMunicipalities.Single();

        Assert.Equal(
            "Pateros",
            pateros.Name);

        Assert.Equal(
            CityMunicipalityType.Municipality,
            pateros.Type);

        Assert.Equal(
            region.Id,
            pateros.RegionId);

        Assert.Null(
            pateros.ProvinceId);
    }


    [Fact]
    public void Import_ShouldAllowSpecialGeographicAreaMunicipality()
    {
        // Arrange
        var rows = new[]
        {
            new PsgcRow(
                "1900000000",
                "Bangsamoro Autonomous Region In Muslim Mindanao (BARMM)",
                "Reg",
                "190000000"),

            new PsgcRow(
                "1999901000",
                "Kapalawan",
                "Mun",
                ""),

            new PsgcRow(
                "1999901001",
                "Kib-Ayao",
                "Bgy",
                "")
        };

        var importer = new PsgcImporter();

        // Act
        var regions = importer.Import(rows);

        // Assert
        Assert.Single(regions);

        var region =
            regions.Single();

        Assert.Equal(
            "Bangsamoro Autonomous Region In Muslim Mindanao (BARMM)",
            region.Name);

        Assert.Empty(
            region.Provinces);

        Assert.Single(
            region.CityMunicipalities);

        var kapalawan =
            region.CityMunicipalities.Single();

        Assert.Equal(
            "Kapalawan",
            kapalawan.Name);

        Assert.Equal(
            CityMunicipalityType.Municipality,
            kapalawan.Type);

        Assert.Equal(
            region.Id,
            kapalawan.RegionId);

        Assert.Null(
            kapalawan.ProvinceId);

        Assert.Single(
            kapalawan.Barangays);

        var barangay =
            kapalawan.Barangays.Single();

        Assert.Equal(
            "Kib-Ayao",
            barangay.Name);

        Assert.Equal(
            kapalawan.Id,
            barangay.CityMunicipalityId);
    }


    [Fact]
    public void Import_ShouldProcessActualPsgcFile()
    {
        // Arrange
        var filePath =
               @"C:\Users\Nelson Villacruz\Downloads\PSGC-2Q-2026-Publication-Datafile.xlsx";


        var reader =
            new PsgcExcelReader();

        var importer =
            new PsgcImporter();

        var rows =
            reader.Read(filePath);

        // Act
        var regions =
            importer.Import(rows);

        // ==================================================
        // BASIC COUNTS
        // ==================================================

        Assert.Equal(
            18,
            regions.Count);

        var allProvinces =
            regions
                .SelectMany(
                    x => x.Provinces)
                .ToList();

        var allCityMunicipalities =
            regions
                .SelectMany(
                    x => x.CityMunicipalities)
                .ToList();

        var importedBarangays =
            allCityMunicipalities
                .SelectMany(
                    x => x.Barangays)
                .ToList();

        Assert.Equal(
            82,
            allProvinces.Count);

        Assert.Equal(
            1642,
            allCityMunicipalities.Count);

        Assert.Equal(
            42010,
            importedBarangays.Count);

        // ==================================================
        // REGION CHECK
        // ==================================================

        Assert.Contains(
            regions,
            x => x.Name.Contains(
                "CALABARZON"));

        Assert.Contains(
            regions,
            x => x.Name.Contains(
                "National Capital Region"));

        // ==================================================
        // RIZAL → TAYTAY → DOLORES
        // ==================================================

        var calabarzon =
            regions.Single(
                x => x.Name.Contains(
                    "CALABARZON"));

        var rizal =
            calabarzon.Provinces.Single(
                x => x.Name == "Rizal");

        var taytay =
            rizal.CityMunicipalities.Single(
                x => x.Name == "Taytay");

        Assert.Equal(
            calabarzon.Id,
            rizal.RegionId);

        Assert.Equal(
            calabarzon.Id,
            taytay.RegionId);

        Assert.Equal(
            rizal.Id,
            taytay.ProvinceId);

        Assert.Contains(
            taytay.Barangays,
            x => x.Name == "Dolores");

        var dolores =
            taytay.Barangays.Single(
                x => x.Name == "Dolores");

        Assert.Equal(
            taytay.Id,
            dolores.CityMunicipalityId);

        // ==================================================
        // HIERARCHY INTEGRITY
        // ==================================================

        // Every Province belongs to its Region.
        Assert.All(
            regions,
            region =>
            {
                Assert.All(
                    region.Provinces,
                    province =>
                    {
                        Assert.Equal(
                            region.Id,
                            province.RegionId);
                    });
            });

        // Every City/Municipality belongs to its Region.
        Assert.All(
            regions,
            region =>
            {
                Assert.All(
                    region.CityMunicipalities,
                    cityMunicipality =>
                    {
                        Assert.Equal(
                            region.Id,
                            cityMunicipality.RegionId);
                    });
            });

        // Every City/Municipality inside a Province
        // references that Province.
        Assert.All(
            regions,
            region =>
            {
                Assert.All(
                    region.Provinces,
                    province =>
                    {
                        Assert.All(
                            province.CityMunicipalities,
                            cityMunicipality =>
                            {
                                Assert.Equal(
                                    province.Id,
                                    cityMunicipality.ProvinceId);
                            });
                    });
            });

        // Every Barangay references the
        // City/Municipality containing it.
        Assert.All(
            allCityMunicipalities,
            cityMunicipality =>
            {
                Assert.All(
                    cityMunicipality.Barangays,
                    barangay =>
                    {
                        Assert.Equal(
                            cityMunicipality.Id,
                            barangay.CityMunicipalityId);
                    });
            });

        // ==================================================
        // NCR
        // ==================================================

        var ncr =
            regions.Single(
                x => x.Name.Contains(
                    "National Capital Region"));

        Assert.Empty(
            ncr.Provinces);

        Assert.All(
            ncr.CityMunicipalities,
            cityMunicipality =>
            {
                Assert.Null(
                    cityMunicipality.ProvinceId);
            });

        // ==================================================
        // BAGUIO
        // ==================================================

        var car =
            regions.Single(
                x => x.Name.Contains(
                    "Cordillera Administrative Region"));

        var baguio =
            car.CityMunicipalities.Single(
                x => x.Name == "City of Baguio");

        Assert.Equal(
            CityMunicipalityType.City,
            baguio.Type);

        Assert.Null(
            baguio.ProvinceId);

        // ==================================================
        // PATEROS
        // ==================================================

        var pateros =
            ncr.CityMunicipalities.Single(
                x => x.Name == "Pateros");

        Assert.Equal(
            CityMunicipalityType.Municipality,
            pateros.Type);

        Assert.Null(
            pateros.ProvinceId);

        // ==================================================
        // BARANGAY COUNT BY REGION
        // ==================================================

        Assert.Equal(
            42010,
            regions
                .SelectMany(
                    x => x.CityMunicipalities)
                .SelectMany(
                    x => x.Barangays)
                .Count());
    }
}