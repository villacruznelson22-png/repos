using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Entities;

public class DeliveryReceipt
{
    public Guid Id { get; private set; }

    public Guid PurchaseOrderId { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public string? Notes { get; private set; }

    public DeliveryReceiptStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;

    public ICollection<DeliveryReceiptLine> Lines { get; private set; }
        = new List<DeliveryReceiptLine>();

    private DeliveryReceipt()
    {
        // Used by EF Core
    }

    public DeliveryReceipt(
        Guid purchaseOrderId,
        DateTime occurredAt,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new DeliveryReceiptDomainException(
                "Purchase order ID cannot be empty.");

        Id = Guid.NewGuid();

        PurchaseOrderId = purchaseOrderId;
        OccurredAt = occurredAt;

        Status = DeliveryReceiptStatus.Draft;
        CreatedAt = DateTime.UtcNow;

        ChangeReferenceNumber(referenceNumber);
        ChangeNotes(notes);
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

    public void AddLine(DeliveryReceiptLine line)
    {
        if (line is null)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt line cannot be null.");

        EnsureEditable();

        if (line.DeliveryReceiptId != Id)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt line does not belong to this delivery receipt.");

        Lines.Add(line);
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt line was not found.");

        Lines.Remove(line);
    }

    public void ChangeLineQuantity(
        Guid lineId,
        decimal quantity)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt line was not found.");

        line.ChangeQuantity(quantity);
    }

    public void ChangeLineUnitCost(
        Guid lineId,
        ValueObjects.Money unitCost)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt line was not found.");

        line.ChangeUnitCost(unitCost);
    }

    public void ChangeLineExpirationDate(
        Guid lineId,
        DateTime? expirationDate)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt line was not found.");

        line.ChangeExpirationDate(expirationDate);
    }

    public void Post()
    {
        if (Status != DeliveryReceiptStatus.Draft)
            throw new DeliveryReceiptDomainException(
                "Only a draft delivery receipt can be posted.");

        if (Lines.Count == 0)
            throw new DeliveryReceiptDomainException(
                "Delivery receipt must contain at least one line.");

        Status = DeliveryReceiptStatus.Posted;
    }

    public void Cancel()
    {
        if (Status != DeliveryReceiptStatus.Draft)
            throw new DeliveryReceiptDomainException(
                "Only a draft delivery receipt can be cancelled.");

        Status = DeliveryReceiptStatus.Cancelled;
    }

    private void EnsureEditable()
    {
        if (Status != DeliveryReceiptStatus.Draft)
            throw new DeliveryReceiptDomainException(
                "Only a draft delivery receipt can be modified.");
    }
}