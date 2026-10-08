using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Products.Queries.GetProductById;

namespace WholesalePOS.Application.Products.Queries.GetProductByBarcode;

public sealed class GetProductByBarcodeHandler
    : IRequestHandler<GetProductByBarcodeQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByBarcodeHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(
        GetProductByBarcodeQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByBarcodeAsync(
            request.Barcode,
            cancellationToken);

        if (product is null)
            throw ProductErrors.NotFoundByBarcode(request.Barcode);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Barcode = product.Barcode?.Value,
            SellingPrice = product.DefaultSellingPrice.Value
        };
    }
}
