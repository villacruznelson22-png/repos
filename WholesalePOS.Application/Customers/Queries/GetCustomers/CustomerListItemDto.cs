namespace WholesalePOS.Application.Customers.Queries.GetCustomers;

public sealed class CustomerListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactNumber { get; set; }

    public bool IsActive { get; set; }
}
