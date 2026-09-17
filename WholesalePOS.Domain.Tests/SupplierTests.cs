using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Tests.Entities;

public class SupplierTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveSupplier()
    {
        var before = DateTime.UtcNow;

        var supplier = new Supplier(
            "JTI Philippines",
            "09170000000",
            "Manila",
            "Cigarette supplier");

        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, supplier.Id);
        Assert.Equal("JTI Philippines", supplier.Name);
        Assert.Equal("09170000000", supplier.ContactNumber);
        Assert.Equal("Manila", supplier.Address);
        Assert.Equal("Cigarette supplier", supplier.Notes);
        Assert.True(supplier.IsActive);
        Assert.InRange(supplier.CreatedAt, before, after);
    }

    [Fact]
    public void Constructor_ShouldTrimValues()
    {
        var supplier = new Supplier(
            "  JTI Philippines  ",
            " 09170000000 ",
            " Manila ",
            " Cigarette supplier ");

        Assert.Equal("JTI Philippines", supplier.Name);
        Assert.Equal("09170000000", supplier.ContactNumber);
        Assert.Equal("Manila", supplier.Address);
        Assert.Equal("Cigarette supplier", supplier.Notes);
    }

    [Fact]
    public void Constructor_ShouldAllowOptionalValuesToBeNull()
    {
        var supplier = new Supplier("JTI Philippines");

        Assert.Null(supplier.ContactNumber);
        Assert.Null(supplier.Address);
        Assert.Null(supplier.Notes);
    }

    [Fact]
    public void Constructor_ShouldConvertWhitespaceOptionalValuesToNull()
    {
        var supplier = new Supplier(
            "JTI Philippines",
            "   ",
            "   ",
            "   ");

        Assert.Null(supplier.ContactNumber);
        Assert.Null(supplier.Address);
        Assert.Null(supplier.Notes);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<SupplierDomainException>(
            () => new Supplier(""));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsWhitespace()
    {
        Assert.Throws<SupplierDomainException>(
            () => new Supplier("   "));
    }

    [Fact]
    public void ChangeName_ShouldUpdateName()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.ChangeName("ABC Distribution");

        Assert.Equal("ABC Distribution", supplier.Name);
    }

    [Fact]
    public void ChangeName_ShouldTrimName()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.ChangeName("  ABC Distribution  ");

        Assert.Equal("ABC Distribution", supplier.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrow_WhenNameIsEmpty()
    {
        var supplier = new Supplier("JTI Philippines");

        Assert.Throws<SupplierDomainException>(
            () => supplier.ChangeName(""));
    }

    [Fact]
    public void ChangeContactNumber_ShouldUpdateContactNumber()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.ChangeContactNumber("09181234567");

        Assert.Equal("09181234567", supplier.ContactNumber);
    }

    [Fact]
    public void ChangeContactNumber_ShouldAllowNull()
    {
        var supplier = new Supplier(
            "JTI Philippines",
            "09181234567");

        supplier.ChangeContactNumber(null);

        Assert.Null(supplier.ContactNumber);
    }

    [Fact]
    public void ChangeAddress_ShouldUpdateAddress()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.ChangeAddress("Quezon City");

        Assert.Equal("Quezon City", supplier.Address);
    }

    [Fact]
    public void ChangeAddress_ShouldAllowNull()
    {
        var supplier = new Supplier(
            "JTI Philippines",
            address: "Quezon City");

        supplier.ChangeAddress(null);

        Assert.Null(supplier.Address);
    }

    [Fact]
    public void ChangeNotes_ShouldUpdateNotes()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.ChangeNotes("Updated notes");

        Assert.Equal("Updated notes", supplier.Notes);
    }

    [Fact]
    public void ChangeNotes_ShouldAllowNull()
    {
        var supplier = new Supplier(
            "JTI Philippines",
            notes: "Old notes");

        supplier.ChangeNotes(null);

        Assert.Null(supplier.Notes);
    }

    [Fact]
    public void Deactivate_ShouldSetSupplierAsInactive()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.Deactivate();

        Assert.False(supplier.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetSupplierAsActive()
    {
        var supplier = new Supplier("JTI Philippines");

        supplier.Deactivate();
        supplier.Activate();

        Assert.True(supplier.IsActive);
    }
}