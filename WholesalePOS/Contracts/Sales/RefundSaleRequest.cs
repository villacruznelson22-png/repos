using System.ComponentModel.DataAnnotations;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Api.Contracts.Sales;

public sealed record RefundSaleRequest(
    [property: Range(typeof(decimal), "0.01", "9999999999999999")]
    decimal Amount,
    PaymentMethod Method,
    [property: Required]
    [property: StringLength(500, MinimumLength = 1)]
    string Reason,
    [property: Required]
    [property: StringLength(100, MinimumLength = 1)]
    string IdempotencyKey,
    [property: StringLength(100)]
    string? ReferenceNumber = null);
