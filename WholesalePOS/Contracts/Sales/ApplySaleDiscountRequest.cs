using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Api.Contracts.Sales;

public sealed record ApplySaleDiscountRequest(
    Guid? SaleLineId,
    DiscountScope Scope,
    DiscountCalculationType CalculationType,
    decimal Value,
    string Reason);
