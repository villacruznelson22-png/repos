using MediatR;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Sales.Queries.GetSales;

public sealed class GetSalesHandler
    : IRequestHandler<GetSalesQuery, PagedResult<SaleListItemDto>>
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<PagedResult<SaleListItemDto>> Handle(
        GetSalesQuery request,
        CancellationToken cancellationToken)
    {
        return await _saleRepository.GetPagedAsync(
            request,
            cancellationToken);
    }
}