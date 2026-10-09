using MediatR;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Sales.Commands.ApplySaleDiscount;

public sealed record ApplySaleDiscountCommand(
    Guid SaleId,
    Guid? SaleLineId,
    DiscountScope Scope,
    DiscountCalculationType CalculationType,
    decimal Value,
    string Reason) : IRequest<Guid>;
