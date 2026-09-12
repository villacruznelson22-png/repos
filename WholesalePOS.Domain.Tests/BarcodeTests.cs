using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class BarcodeTests
{
    [Fact]
    public void ShouldTrimWhitespace()
    {
        var barcode = new Barcode(" 123456789 ");

        Assert.Equal("123456789", barcode.Value);
    }

    [Fact]
    public void ShouldBeEqual_WhenValuesAreTheSame()
    {
        var first = new Barcode("123456789");
        var second = new Barcode("123456789");

        Assert.Equal(first, second);
    }

    [Fact]
    public void ShouldThrow_WhenBarcodeIsEmpty()
    {
        var action = () => new Barcode("");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ShouldThrow_WhenBarcodeIsWhitespace()
    {
        var action = () => new Barcode("   ");

        Assert.Throws<ArgumentException>(action);
    }
}