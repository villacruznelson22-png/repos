using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Domain.Entities;

public class CityMunicipality
{
    public Guid Id { get; private set; }

    public Guid RegionId { get; private set; }

    public Guid? ProvinceId { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public CityMunicipalityType Type { get; private set; }

    public Region Region { get; private set; } = null!;

    public Province? Province { get; private set; }

    public ICollection<Barangay> Barangays
    { get; private set; }
        = new List<Barangay>();

    private CityMunicipality()
    {
    }

    public CityMunicipality(
        Guid regionId,
        Guid? provinceId,
        string code,
        string name,
        CityMunicipalityType type)
    {
        Id = Guid.NewGuid();

        RegionId = regionId;
        ProvinceId = provinceId;
        Code = code;
        Name = name;
        Type = type;
    }
}