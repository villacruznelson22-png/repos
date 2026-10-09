using MediatR;

namespace WholesalePOS.Application.Sales.Commands.RemoveSaleDiscount;

public sealed record RemoveSaleDiscountCommand(
    Guid SaleId,
    Guid DiscountId,
    string Reason) : IRequest;
