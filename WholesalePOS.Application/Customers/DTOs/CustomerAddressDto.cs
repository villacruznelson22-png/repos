namespace WholesalePOS.Application.Customers.DTOs;

public record CustomerAddressDto(
    string Label,
    string? Street,
    Guid BarangayId,
    string? PostalCode,
    string? Landmark,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault
);