using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class Sale
{
    public Guid Id { get; private set; }

    public Guid? CustomerId { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public string? Notes { get; private set; }

    public SaleStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public DateTime? VoidedAt { get; private set; }

    public Guid? VoidedByUserId { get; private set; }

    public string? VoidReason { get; private set; }

    public User? VoidedByUser { get; private set; }

    /// <summary>
    /// Idempotency key of the checkout operation that completed this sale.
    /// It is null until checkout succeeds.
    /// </summary>
    public string? CheckoutIdempotencyKey { get; private set; }

    /// <summary>
    /// SQL Server rowversion used to detect concurrent sale changes.
    /// </summary>
    public byte[] Version { get; private set; } = null!;

    public Customer? Customer { get; private set; }

    public ICollection<SaleLine> Lines { get; private set; }
        = new List<SaleLine>();

    public ICollection<Payment> Payments { get; private set; }
        = new List<Payment>();

    public ICollection<SaleDiscount> Discounts { get; private set; }
        = new List<SaleDiscount>();

    private Sale()
    {
        // Used by EF Core
    }

    public Sale(
        Guid? customerId,
        DateTime occurredAt,
        string? referenceNumber = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();

        CustomerId = customerId;
        OccurredAt = occurredAt;

        Status = SaleStatus.Draft;
        CreatedAt = DateTime.UtcNow;

        ChangeReferenceNumber(referenceNumber);
        ChangeNotes(notes);
    }

    public Money GetSubtotalAmount()
    {
        var subtotal = Lines.Sum(
            line => line.UnitSellingPrice.Value * line.Quantity);

        return new Money(subtotal);
    }

    public Money GetDiscountTotalAmount()
        => new(Discounts.Where(discount => !discount.IsRemoved)
            .Sum(discount => discount.Amount.Value));

    public Money GetTotalAmount()
        => new(Math.Max(0m, GetSubtotalAmount().Value - GetDiscountTotalAmount().Value));

    public void AddDiscount(SaleDiscount discount)
    {
        ArgumentNullException.ThrowIfNull(discount);
        EnsureEditable();

        if (discount.SaleId != Id)
            throw new SaleDomainException("Discount does not belong to this sale.");

        if (discount.IsRemoved)
            throw new SaleDomainException("A removed discount cannot be applied to a sale.");

        if (discount.Scope == DiscountScope.SaleLine)
        {
            if (discount.SaleLineId is not Guid lineId ||
                Lines.All(line => line.Id != lineId))
                throw new SaleDomainException("Discount sale line was not found on this sale.");

            if (Discounts.Any(x => !x.IsRemoved && x.Scope == DiscountScope.Sale))
                throw new SaleDomainException(
                    "Line-item discounts must be applied before sale-wide discounts.");

            var line = Lines.Single(x => x.Id == lineId);
            var alreadyDiscounted = Discounts
                .Where(x => !x.IsRemoved && x.Scope == DiscountScope.SaleLine && x.SaleLineId == lineId)
                .Sum(x => x.Amount.Value);
            var available = line.GetTotalAmount().Value - alreadyDiscounted;

            if (discount.Amount.Value > available)
                throw new SaleDomainException(
                    "Discount amount cannot exceed the remaining line-item amount.");
        }
        else
        {
            var lineDiscounts = Discounts
                .Where(x => !x.IsRemoved && x.Scope == DiscountScope.SaleLine)
                .Sum(x => x.Amount.Value);
            var saleDiscounts = Discounts
                .Where(x => !x.IsRemoved && x.Scope == DiscountScope.Sale)
                .Sum(x => x.Amount.Value);
            var available = GetSubtotalAmount().Value - lineDiscounts - saleDiscounts;

            if (discount.Amount.Value > available)
                throw new SaleDomainException(
                    "Discount amount cannot exceed the remaining sale amount.");
        }

        Discounts.Add(discount);
    }

    public void RemoveDiscount(Guid discountId, Guid removedByUserId, string reason)
    {
        EnsureEditable();

        var discount = Discounts.SingleOrDefault(x => x.Id == discountId);
        if (discount is null)
            throw new SaleDomainException("Discount was not found.");

        discount.Remove(removedByUserId, reason);
    }

    private void EnsureNoDiscountsBeforeChangingLines()
    {
        if (Discounts.Any(discount => !discount.IsRemoved))
            throw new SaleDomainException(
                "Remove the active sale discounts before changing sale lines.");
    }

    public void ChangeOccurredAt(DateTime occurredAt)
    {
        EnsureEditable();

        OccurredAt = occurredAt;
    }

    public void ChangeReferenceNumber(string? referenceNumber)
    {
        EnsureEditable();

        ReferenceNumber =
            string.IsNullOrWhiteSpace(referenceNumber)
                ? null
                : referenceNumber.Trim();
    }

    public void ChangeNotes(string? notes)
    {
        EnsureEditable();

        Notes =
            string.IsNullOrWhiteSpace(notes)
                ? null
                : notes.Trim();
    }

    public void ChangeCustomer(Guid? customerId)
    {
        EnsureEditable();

        CustomerId = customerId;
    }

    public void AddLine(SaleLine line)
    {
        if (line is null)
        {
            throw new SaleDomainException(
                "Sale line cannot be null.");
        }

        EnsureEditable();
        EnsureNoDiscountsBeforeChangingLines();

        if (line.SaleId != Id)
        {
            throw new SaleDomainException(
                "Sale line does not belong to this sale.");
        }

        if (Lines.Any(x => x.ProductId == line.ProductId))
        {
            throw new SaleDomainException(
                "The same product cannot be added more than once to a sale.");
        }

        Lines.Add(line);
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureEditable();
        EnsureNoDiscountsBeforeChangingLines();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
        {
            throw new SaleDomainException(
                "Sale line was not found.");
        }

        // Reversed discounts remain in the audit history and still reference
        // their original sale line. Keep that line to preserve the FK and audit trail.
        if (Discounts.Any(discount => discount.SaleLineId == lineId))
        {
            throw new SaleDomainException(
                "A sale line with discount history cannot be removed.");
        }

        Lines.Remove(line);
    }

    public void ChangeLineQuantity(
        Guid lineId,
        decimal quantity)
    {
        EnsureEditable();
        EnsureNoDiscountsBeforeChangingLines();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
        {
            throw new SaleDomainException(
                "Sale line was not found.");
        }

        line.ChangeQuantity(quantity);
    }

    public void ChangeLineUnitSellingPrice(
        Guid lineId,
        Money unitSellingPrice)
    {
        EnsureEditable();
        EnsureNoDiscountsBeforeChangingLines();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
        {
            throw new SaleDomainException(
                "Sale line was not found.");
        }

        line.ChangeUnitSellingPrice(unitSellingPrice);
    }

    public void AddPayment(Payment payment)
    {
        if (payment is null)
        {
            throw new SaleDomainException(
                "Payment cannot be null.");
        }

        if (Status != SaleStatus.Confirmed)
        {
            throw new SaleDomainException(
                "Payments can only be added to a confirmed sale.");
        }

        if (payment.SaleId != Id)
        {
            throw new SaleDomainException(
                "Payment does not belong to this sale.");
        }

        Payments.Add(payment);
    }

    public void SetCheckoutIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new SaleDomainException(
                "Checkout idempotency key cannot be empty.");

        if (CheckoutIdempotencyKey is not null)
        {
            if (!string.Equals(
                    CheckoutIdempotencyKey,
                    idempotencyKey.Trim(),
                    StringComparison.Ordinal))
            {
                throw new SaleDomainException(
                    "Checkout idempotency key has already been assigned.");
            }

            return;
        }

        CheckoutIdempotencyKey = idempotencyKey.Trim();
    }

    public void Confirm()
    {
        if (Status != SaleStatus.Draft)
        {
            throw new SaleDomainException(
                "Only a draft sale can be confirmed.");
        }

        if (Lines.Count == 0)
        {
            throw new SaleDomainException(
                "Sale must contain at least one line.");
        }

        Status = SaleStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != SaleStatus.Confirmed)
        {
            throw new SaleDomainException(
                "Only a confirmed sale can be completed.");
        }

        if (Lines.Count == 0)
        {
            throw new SaleDomainException(
                "Sale must contain at least one line.");
        }

        Status = SaleStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != SaleStatus.Draft &&
            Status != SaleStatus.Confirmed)
        {
            throw new SaleDomainException(
                "Only a draft or confirmed sale can be cancelled.");
        }

        Status = SaleStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }

    public void Void(Guid voidedByUserId, string reason)
    {
        if (Status != SaleStatus.Completed)
        {
            throw new SaleDomainException(
                "Only a completed sale can be voided.");
        }

        if (voidedByUserId == Guid.Empty)
        {
            throw new SaleDomainException(
                "The user who voids the sale is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new SaleDomainException(
                "A reason is required to void a sale.");
        }

        var normalizedReason = reason.Trim();

        if (normalizedReason.Length > 500)
        {
            throw new SaleDomainException(
                "The void reason cannot exceed 500 characters.");
        }

        Status = SaleStatus.Voided;
        VoidedAt = DateTime.UtcNow;
        VoidedByUserId = voidedByUserId;
        VoidReason = normalizedReason;
    }

    private void EnsureEditable()
    {
        if (Status != SaleStatus.Draft &&
            Status != SaleStatus.Confirmed)
        {
            throw new SaleDomainException(
                "Only a draft or confirmed sale can be modified.");
        }
    }
}
