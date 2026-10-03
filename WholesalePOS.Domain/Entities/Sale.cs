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

    public Customer? Customer { get; private set; }

    public ICollection<SaleLine> Lines { get; private set; }
        = new List<SaleLine>();

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
            throw new SaleDomainException(
                "Sale line cannot be null.");

        EnsureEditable();

        if (line.SaleId != Id)
            throw new SaleDomainException(
                "Sale line does not belong to this sale.");

        if (Lines.Any(x => x.ProductId == line.ProductId))
            throw new SaleDomainException(
                "The same product cannot be added more than once to a sale.");

        Lines.Add(line);
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new SaleDomainException(
                "Sale line was not found.");

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
            throw new SaleDomainException(
                "Sale line was not found.");

        line.ChangeQuantity(quantity);
    }

    public void ChangeLineUnitSellingPrice(
        Guid lineId,
        Money unitSellingPrice)
    {
        EnsureEditable();

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
            throw new SaleDomainException(
                "Sale line was not found.");

        line.ChangeUnitSellingPrice(unitSellingPrice);
    }

    public void Confirm()
    {
        if (Status != SaleStatus.Draft)
            throw new SaleDomainException(
                "Only a draft sale can be confirmed.");

        if (Lines.Count == 0)
            throw new SaleDomainException(
                "Sale must contain at least one line.");

        Status = SaleStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled)
            throw new SaleDomainException(
                "Sale is already cancelled.");

        Status = SaleStatus.Cancelled;
    }

    private void EnsureEditable()
    {
        if (Status != SaleStatus.Draft)
            throw new SaleDomainException(
                "Only a draft sale can be modified.");
    }
}