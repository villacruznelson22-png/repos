namespace WholesalePOS.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? ContactNumber { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Customer()
    {
    }

    public Customer(
        string name,
        string? contactNumber = null)
    {
        Id = Guid.NewGuid();

        Name = name;
        ContactNumber = contactNumber;

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}