namespace WholesalePOS.Application.Customers.Queries.GetCustomerById;

public class CustomerDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactNumber { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public List<CustomerAddressResponseDto> Addresses { get; set; } = new();
}

public class CustomerAddressResponseDto
{
    public Guid Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public string? Street { get; set; }

    public Guid BarangayId { get; set; }

    public string Barangay { get; set; } = string.Empty;

    public string CityMunicipality { get; set; } = string.Empty;

    public string? Province { get; set; }

    public string Region { get; set; } = string.Empty;

    public string? PostalCode { get; set; }

    public string? Landmark { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsDefault { get; set; }
}