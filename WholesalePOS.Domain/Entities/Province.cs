namespace WholesalePOS.Domain.Entities;

public class Province
{
    public Guid Id { get; private set; }

    public Guid RegionId { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public Region Region { get; private set; } = null!;

    public ICollection<CityMunicipality> CityMunicipalities
    { get; private set; }
        = new List<CityMunicipality>();

    private Province()
    {
    }

    public Province(
        Guid regionId,
        string code,
        string name)
    {
        Id = Guid.NewGuid();

        RegionId = regionId;
        Code = code;
        Name = name;
    }
}