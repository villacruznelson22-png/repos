using MediatR;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Common.Models;

namespace WholesalePOS.Application.Products.Queries.GetProducts;

public class GetProductsHandler: IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductListItemDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await _productRepository.GetPagedAsync(
            request,
            cancellationToken);
    }
}