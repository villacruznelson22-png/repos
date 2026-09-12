namespace WholesalePOS.Infrastructure.Imports.Psgc;

public interface IPsgcImporter
{
    Task ImportAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}