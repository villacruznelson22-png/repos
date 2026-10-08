using MediatR;
using WholesalePOS.Application.Common.Models;

namespace WholesalePOS.Application.Products.Queries.GetProducts;

public sealed class GetProductsQuery
    : PaginationRequest,
      IRequest<PagedResult<ProductListItemDto>>
{
    public string? Search { get; init; }

    public bool ActiveOnly { get; init; }
}
