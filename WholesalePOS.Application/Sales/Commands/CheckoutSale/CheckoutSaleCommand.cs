using MediatR;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Sales.Commands.CheckoutSale;

public sealed record CheckoutSaleCommand(
    Guid SaleId,
    string IdempotencyKey,
    IReadOnlyCollection<CheckoutPaymentRequest> Payments,
    bool AllowNegativeInventory = false
) : IRequest;

public sealed record CheckoutPaymentRequest(
    PaymentMethod Method,
    decimal Amount,
    string IdempotencyKey,
    string? ReferenceNumber = null
);