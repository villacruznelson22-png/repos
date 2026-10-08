using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Api.Contracts.Sales;

public sealed record CheckoutSaleRequest(
    string IdempotencyKey,
    IReadOnlyCollection<CheckoutPaymentRequest> Payments,
    bool AllowNegativeInventory = false);

public sealed record CheckoutPaymentRequest(
    PaymentMethod Method,
    decimal Amount,
    string IdempotencyKey,
    string? ReferenceNumber = null);
