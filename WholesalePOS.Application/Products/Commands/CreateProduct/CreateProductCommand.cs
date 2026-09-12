using MediatR;

namespace WholesalePOS.Application.Products.Commands.CreateProduct;

public class CreateProductCommand:IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public decimal DefaultSellingPrice { get; set; }

    public decimal SuggestedRetailPrice { get; set; }
}