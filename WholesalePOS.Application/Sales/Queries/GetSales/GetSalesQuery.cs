using MediatR;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Sales.Queries.GetSales;

public sealed class GetSalesQuery : PaginationRequest, IRequest<PagedResult<SaleListItemDto>>
{
    public SaleStatus? Status { get; init; }

    public bool OpenOnly { get; init; }

    public string? Search { get; init; }
}