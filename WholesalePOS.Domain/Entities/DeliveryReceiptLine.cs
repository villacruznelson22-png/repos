using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class DeliveryReceiptLine
{
    public Guid Id { get; private set; }

    public Guid DeliveryReceiptId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public Money UnitCost { get; private set; } = null!;

    public DateTime? ExpirationDate { get; private set; }

    public DeliveryReceipt DeliveryReceipt { get; private set; } = null!;

    public Product Product { get; private set; } = null!;

    private DeliveryReceiptLine()
    {
        // Used by EF Core
    }

    public DeliveryReceiptLine(
        Guid deliveryReceiptId,
        Guid productId,
        decimal quantity,
        Money unitCost,
        DateTime? expirationDate = null)
    {
        if (deliveryReceiptId == Guid.Empty)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt ID cannot be empty.");

        if (productId == Guid.Empty)
            throw new DeliveryReceiptDomainException(
                "Product ID cannot be empty.");

        if (quantity <= 0)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt quantity must be greater than zero.");

        if (unitCost.Value < 0)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt unit cost cannot be negative.");

        Id = Guid.NewGuid();

        DeliveryReceiptId = deliveryReceiptId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        ExpirationDate = expirationDate;
    }

    public void ChangeQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt quantity must be greater than zero.");

        Quantity = quantity;
    }

    public void ChangeUnitCost(Money unitCost)
    {
        if (unitCost.Value < 0)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt unit cost cannot be negative.");

        UnitCost = unitCost;
    }

    public void ChangeExpirationDate(DateTime? expirationDate)
    {
        ExpirationDate = expirationDate;
    }
}