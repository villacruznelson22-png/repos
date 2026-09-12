using ClosedXML.Excel;

namespace WholesalePOS.Infrastructure.Imports.Psgc;

public sealed class PsgcExcelReader
{
    public IReadOnlyList<PsgcRow> Read(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheet("PSGC");

        var rows = new List<PsgcRow>();

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var code = row.Cell(1).GetString().Trim();

            var name = row.Cell(2).GetString().Trim();

            var correspondenceCode =
                row.Cell(3).GetString().Trim();

            var geographicLevel =
                row.Cell(4).GetString().Trim();

            if (string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(geographicLevel))
            {
                continue;
            }

            rows.Add(new PsgcRow(
                code,
                name,
                geographicLevel,
                correspondenceCode));
        }

        return rows;
    }
}