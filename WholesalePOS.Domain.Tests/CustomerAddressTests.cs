using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Tests.Entities;

public class CustomerAddressTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var customerId = Guid.NewGuid();
        var barangayId = Guid.NewGuid();

        var address = new CustomerAddress(
            customerId,
            "Home",
            "123 Mabini Street",
            barangayId,
            "1920",
            "Near the church",
            14.5631m,
            121.1324m,
            true);

        Assert.NotEqual(Guid.Empty, address.Id);
        Assert.Equal(customerId, address.CustomerId);
        Assert.Equal("Home", address.Label);
        Assert.Equal("123 Mabini Street", address.Street);
        Assert.Equal(barangayId, address.BarangayId);
        Assert.Equal("1920", address.PostalCode);
        Assert.Equal("Near the church", address.Landmark);
        Assert.Equal(14.5631m, address.Latitude);
        Assert.Equal(121.1324m, address.Longitude);
        Assert.True(address.IsDefault);
    }

    [Fact]
    public void Constructor_ShouldTrimStringValues()
    {
        var address = new CustomerAddress(
            Guid.NewGuid(),
            "  Home  ",
            "  123 Mabini Street  ",
            Guid.NewGuid(),
            " 1920 ",
            " Near the church ");

        Assert.Equal("Home", address.Label);
        Assert.Equal("123 Mabini Street", address.Street);
        Assert.Equal("1920", address.PostalCode);
        Assert.Equal("Near the church", address.Landmark);
    }

    [Fact]
    public void Constructor_ShouldAllowOptionalFieldsToBeNull()
    {
        var address = new CustomerAddress(
            Guid.NewGuid(),
            "Home",
            null,
            Guid.NewGuid());

        Assert.Null(address.Street);
        Assert.Null(address.PostalCode);
        Assert.Null(address.Landmark);
        Assert.Null(address.Latitude);
        Assert.Null(address.Longitude);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCustomerIdIsEmpty()
    {
        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            new CustomerAddress(
                Guid.Empty,
                "Home",
                "123 Mabini Street",
                Guid.NewGuid()));

        Assert.Equal(
            "Customer ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLabelIsEmpty()
    {
        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            new CustomerAddress(
                Guid.NewGuid(),
                "",
                "123 Mabini Street",
                Guid.NewGuid()));

        Assert.Equal(
            "Address label cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBarangayIdIsEmpty()
    {
        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            new CustomerAddress(
                Guid.NewGuid(),
                "Home",
                "123 Mabini Street",
                Guid.Empty));

        Assert.Equal(
            "Barangay ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ChangeLabel_ShouldUpdateLabel()
    {
        var address = CreateAddress();

        address.ChangeLabel("Store");

        Assert.Equal("Store", address.Label);
    }

    [Fact]
    public void ChangeLabel_ShouldThrow_WhenLabelIsEmpty()
    {
        var address = CreateAddress();

        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            address.ChangeLabel("   "));

        Assert.Equal(
            "Address label cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ChangeStreet_ShouldAllowNull()
    {
        var address = CreateAddress();

        address.ChangeStreet(null);

        Assert.Null(address.Street);
    }

    [Fact]
    public void ChangeStreet_ShouldTrimValue()
    {
        var address = CreateAddress();

        address.ChangeStreet("  Rizal Street  ");

        Assert.Equal("Rizal Street", address.Street);
    }

    [Fact]
    public void ChangeBarangay_ShouldUpdateBarangayId()
    {
        var address = CreateAddress();
        var newBarangayId = Guid.NewGuid();

        address.ChangeBarangay(newBarangayId);

        Assert.Equal(newBarangayId, address.BarangayId);
    }

    [Fact]
    public void ChangeBarangay_ShouldThrow_WhenBarangayIdIsEmpty()
    {
        var address = CreateAddress();

        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            address.ChangeBarangay(Guid.Empty));

        Assert.Equal(
            "Barangay ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ChangePostalCode_ShouldAllowNull()
    {
        var address = CreateAddress();

        address.ChangePostalCode(null);

        Assert.Null(address.PostalCode);
    }

    [Fact]
    public void ChangeLandmark_ShouldAllowNull()
    {
        var address = CreateAddress();

        address.ChangeLandmark(null);

        Assert.Null(address.Landmark);
    }

    [Fact]
    public void ChangeLocation_ShouldAcceptValidCoordinates()
    {
        var address = CreateAddress();

        address.ChangeLocation(90m, 180m);

        Assert.Equal(90m, address.Latitude);
        Assert.Equal(180m, address.Longitude);
    }

    [Fact]
    public void ChangeLocation_ShouldAcceptMinimumValidCoordinates()
    {
        var address = CreateAddress();

        address.ChangeLocation(-90m, -180m);

        Assert.Equal(-90m, address.Latitude);
        Assert.Equal(-180m, address.Longitude);
    }

    [Fact]
    public void ChangeLocation_ShouldThrow_WhenLatitudeIsBelowMinimum()
    {
        var address = CreateAddress();

        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            address.ChangeLocation(-90.0001m, 120m));

        Assert.Equal(
            "Latitude must be between -90 and 90.",
            exception.Message);
    }

    [Fact]
    public void ChangeLocation_ShouldThrow_WhenLatitudeIsAboveMaximum()
    {
        var address = CreateAddress();

        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            address.ChangeLocation(90.0001m, 120m));

        Assert.Equal(
            "Latitude must be between -90 and 90.",
            exception.Message);
    }

    [Fact]
    public void ChangeLocation_ShouldThrow_WhenLongitudeIsBelowMinimum()
    {
        var address = CreateAddress();

        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            address.ChangeLocation(14m, -180.0001m));

        Assert.Equal(
            "Longitude must be between -180 and 180.",
            exception.Message);
    }

    [Fact]
    public void ChangeLocation_ShouldThrow_WhenLongitudeIsAboveMaximum()
    {
        var address = CreateAddress();

        var exception = Assert.Throws<CustomerAddressDomainException>(() =>
            address.ChangeLocation(14m, 180.0001m));

        Assert.Equal(
            "Longitude must be between -180 and 180.",
            exception.Message);
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultToTrue()
    {
        var address = CreateAddress();

        address.SetAsDefault();

        Assert.True(address.IsDefault);
    }

    [Fact]
    public void RemoveAsDefault_ShouldSetIsDefaultToFalse()
    {
        var address = CreateAddress(true);

        address.RemoveAsDefault();

        Assert.False(address.IsDefault);
    }

    private static CustomerAddress CreateAddress(bool isDefault = false)
    {
        return new CustomerAddress(
            Guid.NewGuid(),
            "Home",
            "123 Mabini Street",
            Guid.NewGuid(),
            "1920",
            "Near the church",
            14.5631m,
            121.1324m,
            isDefault);
    }
}