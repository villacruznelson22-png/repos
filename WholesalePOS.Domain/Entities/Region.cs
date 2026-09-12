namespace WholesalePOS.Domain.Entities;

public class Region
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public ICollection<Province> Provinces { get; private set; }
        = new List<Province>();

    public ICollection<CityMunicipality> CityMunicipalities
    { get; private set; }
        = new List<CityMunicipality>();

    private Region()
    {
    }

    public Region(
        string code,
        string name)
    {
        Id = Guid.NewGuid();

        Code = code;
        Name = name;
    }
}