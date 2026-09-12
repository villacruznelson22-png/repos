using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace WholesalePOS.Infrastructure.Imports.Psgc;

public static class PsgcCodeParser
{

    //Region = first 2 digits + 8 zeroes
    //Province = first 5 digits + 5 zeroes
    //City/Municipality = first 7 digits + 3 zeroes
    //Barangay's parent = first 7 digits + 3 zeroes

    public static string GetRegionCode(string psgcCode)
    {
        Validate(psgcCode);

        return $"{psgcCode[..2]}00000000";
    }

    public static string GetProvinceCode(string psgcCode)
    {
        Validate(psgcCode);

        return $"{psgcCode[..5]}00000";
    }

    public static string GetCityMunicipalityCode(
        string psgcCode)
    {
        Validate(psgcCode);

        return $"{psgcCode[..7]}000";
    }

    private static void Validate(string psgcCode)
    {
        if (string.IsNullOrWhiteSpace(psgcCode))
        {
            throw new ArgumentException(
                "PSGC code cannot be empty.",
                nameof(psgcCode));
        }

        if (psgcCode.Length != 10)
        {
            throw new ArgumentException(
                "PSGC code must contain exactly 10 characters.",
                nameof(psgcCode));
        }
    }

    public static string GetSubMunicipalityParentCode(
     string psgcCode)
    {
        Validate(psgcCode);

        return $"{psgcCode[..5]}00000";
    }

}