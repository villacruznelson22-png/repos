using MediatR;
using WholesalePOS.Application.Products.Queries.GetProductById;

namespace WholesalePOS.Application.Products.Queries.GetProductByBarcode;

public sealed record GetProductByBarcodeQuery(
    string Barcode
) : IRequest<ProductDto>;
