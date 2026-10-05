using MediatR;

namespace WholesalePOS.Application.Sales.Commands.CancelSale;

public sealed record CancelSaleCommand(
    Guid SaleId
) : IRequest;