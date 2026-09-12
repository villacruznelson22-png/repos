using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

public sealed class Barcode : ValueObject
{
    public string Value { get; }

    public Barcode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Barcode cannot be empty.");

        Value = value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}