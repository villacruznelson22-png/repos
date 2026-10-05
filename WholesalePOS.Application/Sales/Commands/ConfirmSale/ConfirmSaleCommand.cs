using MediatR;

namespace WholesalePOS.Application.Sales.Commands.ConfirmSale;

public sealed record ConfirmSaleCommand(
    Guid SaleId
) : IRequest;