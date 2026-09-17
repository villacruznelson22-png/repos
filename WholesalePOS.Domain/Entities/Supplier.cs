using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Entities;

public class Supplier
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? ContactNumber { get; private set; }

    public string? Address { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Supplier()
    {
        // Used by EF Core
    }

    public Supplier(
        string name,
        string? contactNumber = null,
        string? address = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();

        ChangeName(name);
        ChangeContactNumber(contactNumber);
        ChangeAddress(address);
        ChangeNotes(notes);

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new SupplierDomainException(
                "Supplier name cannot be empty.");

        Name = name.Trim();
    }

    public void ChangeContactNumber(string? contactNumber)
    {
        ContactNumber = string.IsNullOrWhiteSpace(contactNumber)
            ? null
            : contactNumber.Trim();
    }

    public void ChangeAddress(string? address)
    {
        Address = string.IsNullOrWhiteSpace(address)
            ? null
            : address.Trim();
    }

    public void ChangeNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}