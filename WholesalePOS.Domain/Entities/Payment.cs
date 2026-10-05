using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTime PaidAt { get; private set; }
    public string? ReferenceNumber { get; private set; }

    public Sale Sale { get; private set; } = null!;

    private Payment()
    {
        // Used by EF Core
    }

    public Payment(
        Guid saleId,
        PaymentMethod method,
        Money amount,
        DateTime paidAt,
        string? referenceNumber = null)
    {
        if (saleId == Guid.Empty)
            throw new SaleDomainException("Sale ID cannot be empty.");

        if (amount.Value <= 0)
            throw new SaleDomainException(
                "Payment amount must be greater than zero.");

        Id = Guid.NewGuid();
        SaleId = saleId;
        Method = method;
        Amount = amount;
        PaidAt = paidAt;

        ReferenceNumber =
            string.IsNullOrWhiteSpace(referenceNumber)
                ? null
                : referenceNumber.Trim();
    }
}
