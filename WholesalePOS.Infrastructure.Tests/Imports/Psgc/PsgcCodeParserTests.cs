using WholesalePOS.Infrastructure.Imports.Psgc;

namespace WholesalePOS.Infrastructure.Tests.Imports.Psgc;

public class PsgcCodeParserTests
{
    [Fact]
    public void ShouldInspectActualPsgcCodes()
    {
        // Arrange
        var filePath =
             @"C:\Users\Nelson Villacruz\Downloads\PSGC-2Q-2026-Publication-Datafile.xlsx";

        var reader = new PsgcExcelReader();

        var rows = reader.Read(filePath);

        var region = rows.First(x =>
            x.GeographicLevel == "Reg" &&
            x.Name.Contains("CALABARZON"));

        var province = rows.First(x =>
            x.GeographicLevel == "Prov" &&
            x.Name == "Rizal");

        var municipality = rows.First(x =>
            x.GeographicLevel == "Mun" &&
            x.Name == "Taytay");

        //var barangay = rows.First(x =>
        //    x.GeographicLevel == "Bgy" &&
        //    x.Name == "San Isidro");

        var barangay = rows.First(x =>
    x.GeographicLevel == "Bgy" &&
    x.Code.StartsWith("0405813"));

        // Output
        Console.WriteLine(
            $"REGION       : {region.Code} | Corr: {region.CorrespondenceCode} | {region.Name}");

        Console.WriteLine(
            $"PROVINCE     : {province.Code} | Corr: {province.CorrespondenceCode} | {province.Name}");

        Console.WriteLine(
            $"MUNICIPALITY : {municipality.Code} | Corr: {municipality.CorrespondenceCode} | {municipality.Name}");

        Console.WriteLine(
            $"BARANGAY     : {barangay.Code} | Corr: {barangay.CorrespondenceCode} | {barangay.Name}");

        Assert.NotEmpty(region.Code);
        Assert.NotEmpty(province.Code);
        Assert.NotEmpty(municipality.Code);
        Assert.NotEmpty(barangay.Code);
    }

    [Fact]
    public void Read_ShouldShowNcrHierarchy()
    {
        // Arrange
        var filePath =
             @"C:\Users\Nelson Villacruz\Downloads\PSGC-2Q-2026-Publication-Datafile.xlsx";

        var reader = new PsgcExcelReader();

        var rows = reader.Read(filePath);

        // Act
        var region = rows.First(x =>
            x.GeographicLevel == "Reg" &&
            x.Name.Contains("National Capital Region"));

        var city = rows.First(x =>
            x.GeographicLevel == "City" &&
            x.Name.Contains("Quezon"));

        var barangay = rows.First(x =>
            x.GeographicLevel == "Bgy" &&
            x.Code.StartsWith(city.Code[..7]));

        // Output
        Console.WriteLine(
            $"REGION   : {region.Code} | Corr: {region.CorrespondenceCode} | {region.Name}");

        Console.WriteLine(
            $"CITY     : {city.Code} | Corr: {city.CorrespondenceCode} | {city.Name}");

        Console.WriteLine(
            $"BARANGAY : {barangay.Code} | Corr: {barangay.CorrespondenceCode} | {barangay.Name}");

        Assert.NotEmpty(region.Code);
        Assert.NotEmpty(city.Code);
        Assert.NotEmpty(barangay.Code);
    }

    [Fact]
    public void GetRegionCode_ShouldReturnRegionCode()
    {
        var result =
            PsgcCodeParser.GetRegionCode(
                "0405813001");

        Assert.Equal(
            "0400000000",
            result);
    }

    [Fact]
    public void GetProvinceCode_ShouldReturnProvinceCode()
    {
        var result =
            PsgcCodeParser.GetProvinceCode(
                "0405813001");

        Assert.Equal(
            "0405800000",
            result);
    }

    [Fact]
    public void GetCityMunicipalityCode_ShouldReturnCityMunicipalityCode()
    {
        var result =
            PsgcCodeParser.GetCityMunicipalityCode(
                "0405813001");

        Assert.Equal(
            "0405813000",
            result);
    }

    [Fact]
    public void GetRegionCode_ShouldWorkForNcr()
    {
        var result =
            PsgcCodeParser.GetRegionCode(
                "1381300001");

        Assert.Equal(
            "1300000000",
            result);
    }

    [Fact]
    public void GetCityMunicipalityCode_ShouldWorkForNcr()
    {
        var result =
            PsgcCodeParser.GetCityMunicipalityCode(
                "1381300001");

        Assert.Equal(
            "1381300000",
            result);
    }
}