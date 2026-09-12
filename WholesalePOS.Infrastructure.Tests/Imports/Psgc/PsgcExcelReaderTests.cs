using WholesalePOS.Infrastructure.Imports.Psgc;

namespace WholesalePOS.Infrastructure.Tests.Imports.Psgc;

public class PsgcExcelReaderTests
{
    [Fact]
    public void Read_ShouldReadPsgcRows()
    {
        // Arrange
        var filePath =
            @"C:\Users\Nelson Villacruz\Downloads\PSGC-2Q-2026-Publication-Datafile.xlsx";

        var reader = new PsgcExcelReader();

        // Act
        var rows = reader.Read(filePath);

        // Assert
        Assert.NotEmpty(rows);


        Assert.Equal(
            18,
            rows.Count(x => x.GeographicLevel == "Reg"));

        Assert.Equal(
            82,
            rows.Count(x => x.GeographicLevel == "Prov"));

        Assert.Equal(
            149,
            rows.Count(x => x.GeographicLevel == "City"));

        Assert.Equal(
            1493,
            rows.Count(x => x.GeographicLevel == "Mun"));

        Assert.Equal(
            42010,
            rows.Count(x => x.GeographicLevel == "Bgy"));

        Assert.Contains(
            rows,
            x => x.GeographicLevel == "SubMun");

        Assert.Contains(
            rows,
            x => !string.IsNullOrWhiteSpace(
                x.CorrespondenceCode));
    }

    [Fact]
    public void Read_ShouldShowPsgcHierarchy()
    {
        // Arrange
        var filePath =
             @"C:\Users\Nelson Villacruz\Downloads\PSGC-2Q-2026-Publication-Datafile.xlsx";

        var reader = new PsgcExcelReader();

        // Act
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

        var barangay = rows.First(x =>
            x.GeographicLevel == "Bgy" &&
            x.Name == "San Isidro");

        // Output
        Console.WriteLine(
            $"REGION: {region.Code} | {region.Name} | Corr: {region.CorrespondenceCode}");

        Console.WriteLine(
            $"PROVINCE: {province.Code} | {province.Name} | Corr: {province.CorrespondenceCode}");

        Console.WriteLine(
            $"MUNICIPALITY: {municipality.Code} | {municipality.Name} | Corr: {municipality.CorrespondenceCode}");

        Console.WriteLine(
            $"BARANGAY: {barangay.Code} | {barangay.Name} | Corr: {barangay.CorrespondenceCode}");
    }
}