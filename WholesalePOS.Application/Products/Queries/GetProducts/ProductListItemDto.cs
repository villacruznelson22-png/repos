namespace WholesalePOS.Application.Products.Queries.GetProducts;

public class ProductListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public decimal SellingPrice { get; set; }
}