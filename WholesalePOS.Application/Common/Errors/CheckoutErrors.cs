using System;
using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class CheckoutErrors
{
    public static ConflictException AlreadyProcessed(Guid saleId)
        => new($"Sale '{saleId}' has already been completed.");

    public static ConflictException IdempotencyKeyConflict()
        => new("The checkout idempotency key is already associated with another checkout.");

    public static ConflictException PaymentTotalMismatch(decimal saleTotal, decimal paymentTotal)
        => new(
            $"Payment total '{paymentTotal:0.00}' does not match sale total '{saleTotal:0.00}'.");

    public static ConflictException InsufficientInventory(Guid productId)
        => new($"Insufficient inventory for product '{productId}'.");

    public static ConflictException InventoryCostUnavailable(Guid productId)
        => new(
            $"No inventory cost is available for product '{productId}'. " +
            "Set a current inventory cost or an explicit fallback inventory cost before using negative inventory.");

    public static ConflictException DuplicatePaymentIdempotencyKey(string idempotencyKey)
        => new($"Payment idempotency key '{idempotencyKey}' is duplicated in the checkout request.");
}