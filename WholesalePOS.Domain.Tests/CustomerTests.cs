using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var customer = new Customer(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Juan Dela Cruz", customer.Name);
        Assert.Equal("09171234567", customer.ContactNumber);
        Assert.Equal("Wholesale customer", customer.Notes);
        Assert.True(customer.IsActive);
        Assert.Empty(customer.Addresses);
    }

    [Fact]
    public void Constructor_ShouldTrimStringValues()
    {
        var customer = new Customer(
            "  Juan Dela Cruz  ",
            " 09171234567 ",
            " Wholesale customer ");

        Assert.Equal("Juan Dela Cruz", customer.Name);
        Assert.Equal("09171234567", customer.ContactNumber);
        Assert.Equal("Wholesale customer", customer.Notes);
    }

    [Fact]
    public void Constructor_ShouldAllowOptionalFieldsToBeNull()
    {
        var customer = new Customer(
            "Juan Dela Cruz");

        Assert.Null(customer.ContactNumber);
        Assert.Null(customer.Notes);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<CustomerDomainException>(() =>
            new Customer(
                "",
                "09171234567"));

        Assert.Equal(
            "Customer name cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ChangeName_ShouldUpdateName()
    {
        var customer = CreateCustomer();

        customer.ChangeName("Pedro Santos");

        Assert.Equal("Pedro Santos", customer.Name);
    }

    [Fact]
    public void ChangeName_ShouldTrimName()
    {
        var customer = CreateCustomer();

        customer.ChangeName("  Pedro Santos  ");

        Assert.Equal("Pedro Santos", customer.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrow_WhenNameIsEmpty()
    {
        var customer = CreateCustomer();

        var exception = Assert.Throws<CustomerDomainException>(() =>
            customer.ChangeName("   "));

        Assert.Equal(
            "Customer name cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ChangeContactNumber_ShouldUpdateContactNumber()
    {
        var customer = CreateCustomer();

        customer.ChangeContactNumber("09987654321");

        Assert.Equal("09987654321", customer.ContactNumber);
    }

    [Fact]
    public void ChangeContactNumber_ShouldAllowNull()
    {
        var customer = CreateCustomer();

        customer.ChangeContactNumber(null);

        Assert.Null(customer.ContactNumber);
    }

    [Fact]
    public void ChangeContactNumber_ShouldTrimValue()
    {
        var customer = CreateCustomer();

        customer.ChangeContactNumber(" 09987654321 ");

        Assert.Equal("09987654321", customer.ContactNumber);
    }

    [Fact]
    public void ChangeNotes_ShouldUpdateNotes()
    {
        var customer = CreateCustomer();

        customer.ChangeNotes("Updated notes");

        Assert.Equal("Updated notes", customer.Notes);
    }

    [Fact]
    public void ChangeNotes_ShouldAllowNull()
    {
        var customer = CreateCustomer();

        customer.ChangeNotes(null);

        Assert.Null(customer.Notes);
    }

    [Fact]
    public void ChangeNotes_ShouldTrimValue()
    {
        var customer = CreateCustomer();

        customer.ChangeNotes(" Updated notes ");

        Assert.Equal("Updated notes", customer.Notes);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var customer = CreateCustomer();

        customer.Deactivate();

        Assert.False(customer.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var customer = CreateCustomer();

        customer.Deactivate();
        customer.Activate();

        Assert.True(customer.IsActive);
    }

    [Fact]
    public void AddAddress_ShouldAddAddress()
    {
        var customer = CreateCustomer();
        var address = CreateAddress(customer.Id);

        customer.AddAddress(address);

        Assert.Single(customer.Addresses);
        Assert.Contains(address, customer.Addresses);
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenAddressIsNull()
    {
        var customer = CreateCustomer();

        var exception = Assert.Throws<CustomerDomainException>(() =>
            customer.AddAddress(null!));

        Assert.Equal(
            "Customer address cannot be null.",
            exception.Message);
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenAddressBelongsToAnotherCustomer()
    {
        var customer = CreateCustomer();
        var address = CreateAddress(Guid.NewGuid());

        var exception = Assert.Throws<CustomerDomainException>(() =>
            customer.AddAddress(address));

        Assert.Equal(
            "Customer address does not belong to this customer.",
            exception.Message);
    }

    [Fact]
    public void AddAddress_ShouldSetDefaultAddress()
    {
        var customer = CreateCustomer();
        var address = CreateAddress(customer.Id, true);

        customer.AddAddress(address);

        Assert.True(address.IsDefault);
    }

    [Fact]
    public void AddAddress_ShouldRemovePreviousDefaultAddress()
    {
        var customer = CreateCustomer();

        var homeAddress = CreateAddress(customer.Id, true);
        var storeAddress = CreateAddress(customer.Id, true);

        customer.AddAddress(homeAddress);
        customer.AddAddress(storeAddress);

        Assert.False(homeAddress.IsDefault);
        Assert.True(storeAddress.IsDefault);
    }

    [Fact]
    public void RemoveAddress_ShouldRemoveAddress()
    {
        var customer = CreateCustomer();
        var address = CreateAddress(customer.Id);

        customer.AddAddress(address);
        customer.RemoveAddress(address.Id);

        Assert.Empty(customer.Addresses);
    }

    [Fact]
    public void RemoveAddress_ShouldThrow_WhenAddressDoesNotExist()
    {
        var customer = CreateCustomer();

        var exception = Assert.Throws<CustomerDomainException>(() =>
            customer.RemoveAddress(Guid.NewGuid()));

        Assert.Equal(
            "Customer address was not found.",
            exception.Message);
    }

    [Fact]
    public void SetDefaultAddress_ShouldSetSelectedAddressAsDefault()
    {
        var customer = CreateCustomer();

        var homeAddress = CreateAddress(customer.Id);
        var storeAddress = CreateAddress(customer.Id);

        customer.AddAddress(homeAddress);
        customer.AddAddress(storeAddress);

        customer.SetDefaultAddress(storeAddress.Id);

        Assert.False(homeAddress.IsDefault);
        Assert.True(storeAddress.IsDefault);
    }

    [Fact]
    public void SetDefaultAddress_ShouldRemovePreviousDefaultAddress()
    {
        var customer = CreateCustomer();

        var homeAddress = CreateAddress(customer.Id);
        var storeAddress = CreateAddress(customer.Id);

        customer.AddAddress(homeAddress);
        customer.AddAddress(storeAddress);

        customer.SetDefaultAddress(homeAddress.Id);
        customer.SetDefaultAddress(storeAddress.Id);

        Assert.False(homeAddress.IsDefault);
        Assert.True(storeAddress.IsDefault);
    }

    [Fact]
    public void SetDefaultAddress_ShouldThrow_WhenAddressDoesNotExist()
    {
        var customer = CreateCustomer();

        var exception = Assert.Throws<CustomerDomainException>(() =>
            customer.SetDefaultAddress(Guid.NewGuid()));

        Assert.Equal(
            "Customer address was not found.",
            exception.Message);
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer");
    }

    private static CustomerAddress CreateAddress(
        Guid customerId,
        bool isDefault = false)
    {
        return new CustomerAddress(
            customerId,
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