using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Entities;

public class CustomerAddress
{
    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Label { get; private set; } = null!;

    public string? Street { get; private set; }

    public Guid BarangayId { get; private set; }

    public string? PostalCode { get; private set; }

    public string? Landmark { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public bool IsDefault { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public Barangay Barangay { get; private set; } = null!;

    private CustomerAddress()
    {
        // Used by EF Core
    }

    public CustomerAddress(
        Guid customerId,
        string label,
        string? street,
        Guid barangayId,
        string? postalCode = null,
        string? landmark = null,
        decimal? latitude = null,
        decimal? longitude = null,
        bool isDefault = false)
    {
        Id = Guid.NewGuid();

        if (customerId == Guid.Empty)
            throw new CustomerAddressDomainException(
                "Customer ID cannot be empty.");

        CustomerId = customerId;

        ChangeLabel(label);
        ChangeStreet(street);
        ChangeBarangay(barangayId);
        ChangePostalCode(postalCode);
        ChangeLandmark(landmark);
        ChangeLocation(latitude, longitude);

        IsDefault = isDefault;
    }

    public void ChangeLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new CustomerAddressDomainException(
                "Address label cannot be empty.");

        Label = label.Trim();
    }

    public void ChangeStreet(string? street)
    {
        Street = string.IsNullOrWhiteSpace(street)
            ? null
            : street.Trim();
    }

    public void ChangeBarangay(Guid barangayId)
    {
        if (barangayId == Guid.Empty)
            throw new CustomerAddressDomainException(
                "Barangay ID cannot be empty.");

        BarangayId = barangayId;
    }

    public void ChangePostalCode(string? postalCode)
    {
        PostalCode = string.IsNullOrWhiteSpace(postalCode)
            ? null
            : postalCode.Trim();
    }

    public void ChangeLandmark(string? landmark)
    {
        Landmark = string.IsNullOrWhiteSpace(landmark)
            ? null
            : landmark.Trim();
    }

    public void ChangeLocation(decimal? latitude, decimal? longitude)
    {
        if (latitude is < -90 or > 90)
            throw new CustomerAddressDomainException(
                "Latitude must be between -90 and 90.");

        if (longitude is < -180 or > 180)
            throw new CustomerAddressDomainException(
                "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void RemoveAsDefault()
    {
        IsDefault = false;
    }
}