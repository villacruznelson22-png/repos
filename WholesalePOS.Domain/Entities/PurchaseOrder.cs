using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class PurchaseOrder
{
    public Guid Id { get; private set; }

    public Guid SupplierId { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public string? Notes { get; private set; }

    public PurchaseOrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Supplier Supplier { get; private set; } = null!;

    public ICollection<PurchaseOrderLine> Lines { get; private set; }
        = new List<PurchaseOrderLine>();

    private PurchaseOrder()
    {
        // Used by EF Core
    }

    public PurchaseOrder(
    Guid supplierId,
    DateTime occurredAt,
    string? referenceNumber = null,
    string? notes = null)
    {
        if (supplierId == Guid.Empty)
            throw new PurchaseOrderDomainException(
                "Supplier ID cannot be empty.");

        Id = Guid.NewGuid();
        SupplierId = supplierId;
        OccurredAt = occurredAt;

        Status = PurchaseOrderStatus.Draft;
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

    public void AddLine(PurchaseOrderLine line)
    {
        if (line is null)
            throw new PurchaseOrderDomainException(
                "Purchase order line cannot be null.");

        EnsureEditable();

        if (line.PurchaseOrderId != Id)
            throw new PurchaseOrderDomainException(
                "Purchase order line does not belong to this purchase order.");

        if (Lines.Any(x => x.ProductId == line.ProductId))
            throw new PurchaseOrderDomainException(
                "The same product cannot be added more than once to a purchase order.");

        Lines.Add(line);
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new PurchaseOrderDomainException(
                "Purchase order line was not found.");

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
            throw new PurchaseOrderDomainException(
                "Purchase order line was not found.");

        line.ChangeQuantity(quantity);
    }

    public void ChangeLineUnitCost(
        Guid lineId,
        Money unitCost)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new PurchaseOrderDomainException(
                "Purchase order line was not found.");

        line.ChangeUnitCost(unitCost);
    }

    public void Post()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new PurchaseOrderDomainException(
                "Only a draft purchase order can be posted.");

        if (Lines.Count == 0)
            throw new PurchaseOrderDomainException(
                "Purchase order must contain at least one line.");

        Status = PurchaseOrderStatus.Posted;
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Cancelled)
            throw new PurchaseOrderDomainException(
                "Purchase order is already cancelled.");

        Status = PurchaseOrderStatus.Cancelled;
    }

    private void EnsureEditable()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new PurchaseOrderDomainException(
                "Only a draft purchase order can be modified.");
    }
}