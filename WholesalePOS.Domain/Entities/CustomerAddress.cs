namespace WholesalePOS.Domain.Entities;

public class CustomerAddress
{
    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Label { get; private set; } = null!;

    public string Street { get; private set; } = null!;

    public string Barangay { get; private set; } = null!;

    public string CityMunicipality { get; private set; } = null!;

    public string Province { get; private set; } = null!;

    public string? PostalCode { get; private set; }

    public string? Landmark { get; private set; }

    public bool IsDefault { get; private set; }

    private CustomerAddress()
    {
    }

    public CustomerAddress(
        Guid customerId,
        string label,
        string street,
        string barangay,
        string cityMunicipality,
        string province,
        string? postalCode = null,
        string? landmark = null,
        bool isDefault = false)
    {
        Id = Guid.NewGuid();

        CustomerId = customerId;
        Label = label;
        Street = street;
        Barangay = barangay;
        CityMunicipality = cityMunicipality;
        Province = province;
        PostalCode = postalCode;
        Landmark = landmark;
        IsDefault = isDefault;
    }
}