using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? ContactNumber { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ICollection<CustomerAddress> Addresses
    { get; private set; }
        = new List<CustomerAddress>();

    private Customer()
    {
        // Used by EF Core
    }

    public Customer(
        string name,
        string? contactNumber = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();

        ChangeName(name);
        ChangeContactNumber(contactNumber);
        ChangeNotes(notes);

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CustomerDomainException(
                "Customer name cannot be empty.");

        Name = name.Trim();
    }

    public void ChangeContactNumber(string? contactNumber)
    {
        ContactNumber = string.IsNullOrWhiteSpace(contactNumber)
            ? null
            : contactNumber.Trim();
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

    public void AddAddress(CustomerAddress address)
    {
        if (address is null)
            throw new CustomerDomainException(
                "Customer address cannot be null.");

        if (address.CustomerId != Id)
            throw new CustomerDomainException(
                "Customer address does not belong to this customer.");

        Addresses.Add(address);

        if (address.IsDefault)
        {
            SetDefaultAddress(address.Id);
        }
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = Addresses
            .SingleOrDefault(a => a.Id == addressId);

        if (address is null)
            throw new CustomerDomainException(
                "Customer address was not found.");

        Addresses.Remove(address);
    }

    public void SetDefaultAddress(Guid addressId)
    {
        var address = Addresses
            .SingleOrDefault(a => a.Id == addressId);

        if (address is null)
            throw new CustomerDomainException(
                "Customer address was not found.");

        foreach (var existingAddress in Addresses)
        {
            existingAddress.RemoveAsDefault();
        }

        address.SetAsDefault();
    }
}