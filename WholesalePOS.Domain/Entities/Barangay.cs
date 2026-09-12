namespace WholesalePOS.Domain.Entities;

public class Barangay
{
    public Guid Id { get; private set; }

    public Guid CityMunicipalityId { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public CityMunicipality CityMunicipality { get; private set; }
        = null!;

    private Barangay()
    {
    }

    public Barangay(
        Guid cityMunicipalityId,
        string code,
        string name)
    {
        Id = Guid.NewGuid();

        CityMunicipalityId = cityMunicipalityId;
        Code = code;
        Name = name;
    }
}