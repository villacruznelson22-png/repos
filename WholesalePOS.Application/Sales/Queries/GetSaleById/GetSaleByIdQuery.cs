using MediatR;

namespace WholesalePOS.Application.Sales.Queries.GetSaleById;

public sealed record GetSaleByIdQuery(
    Guid SaleId) : IRequest<SaleDto>;
