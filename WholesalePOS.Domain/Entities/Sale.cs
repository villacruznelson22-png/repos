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

    public Customer? Customer { get; private set; }

    public ICollection<SaleLine> Lines { get; private set; }
        = new List<SaleLine>();

    public ICollection<Payment> Payments { get; private set; }
        = new List<Payment>();

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
        {
            throw new SaleDomainException(
                "Sale line cannot be null.");
        }

        EnsureEditable();

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

        var line = Lines.SingleOrDefault(
            x => x.Id == lineId);

        if (line is null)
        {
            throw new SaleDomainException(
                "Sale line was not found.");
        }

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

    public void Void()
    {
        if (Status != SaleStatus.Completed)
        {
            throw new SaleDomainException(
                "Only a completed sale can be voided.");
        }

        Status = SaleStatus.Voided;
        VoidedAt = DateTime.UtcNow;
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