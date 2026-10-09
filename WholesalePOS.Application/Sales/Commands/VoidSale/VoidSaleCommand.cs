using MediatR;

namespace WholesalePOS.Application.Sales.Commands.VoidSale;

public sealed record VoidSaleCommand(
    Guid SaleId,
    string Reason) : IRequest;
