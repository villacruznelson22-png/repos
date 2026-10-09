using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

/// <summary>
/// Records money returned against a completed or voided sale.
/// Refund records are append-only; original payments are never modified.
/// </summary>
public class SaleRefund
{
    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public DateTime RefundedAt { get; private set; }
    public Guid RefundedByUserId { get; private set; }
    public string Reason { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = null!;
    public string? ReferenceNumber { get; private set; }
    public Sale Sale { get; private set; } = null!;
    public User RefundedByUser { get; private set; } = null!;

    private SaleRefund() { }

    public SaleRefund(
        Guid saleId,
        Money amount,
        PaymentMethod method,
        DateTime refundedAt,
        Guid refundedByUserId,
        string reason,
        string idempotencyKey,
        string? referenceNumber = null)
    {
        if (saleId == Guid.Empty)
            throw new SaleDomainException("Sale ID cannot be empty.");
        ArgumentNullException.ThrowIfNull(amount);
        if (amount.Value <= 0)
            throw new SaleDomainException("Refund amount must be greater than zero.");
        if (refundedByUserId == Guid.Empty)
            throw new SaleDomainException("The user recording the refund is required.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new SaleDomainException("A refund reason is required.");
        if (reason.Trim().Length > 500)
            throw new SaleDomainException("Refund reason cannot exceed 500 characters.");
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new SaleDomainException("Refund idempotency key cannot be empty.");
        if (idempotencyKey.Trim().Length > 100)
            throw new SaleDomainException("Refund idempotency key cannot exceed 100 characters.");

        Id = Guid.NewGuid();
        SaleId = saleId;
        Amount = amount;
        Method = method;
        RefundedAt = refundedAt;
        RefundedByUserId = refundedByUserId;
        Reason = reason.Trim();
        IdempotencyKey = idempotencyKey.Trim();
        ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber)
            ? null
            : referenceNumber.Trim();
    }
}
