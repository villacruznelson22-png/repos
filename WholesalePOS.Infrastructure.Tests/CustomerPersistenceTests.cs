using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests;



public class CustomerPersistenceTests
{
    [Fact]
    [TestDatabase]
    public async Task ShouldPersistCustomerWithAddressAndPsgcHierarchy()
    {
        // Arrange
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var region =
            new Region(
                "TEST-REGION",
                "Test Region");

        var province =
            new Province(
                region.Id,
                "TEST-PROVINCE",
                "Test Province");

        var cityMunicipality =
            new CityMunicipality(
                region.Id,
                province.Id,
                "TEST-CITY",
                "Test City",
                WholesalePOS.Domain.Enums.CityMunicipalityType.Municipality);

        var barangay =
            new Barangay(
                cityMunicipality.Id,
                "TEST-BARANGAY",
                "Test Barangay");

        context.Regions.Add(region);
        context.Provinces.Add(province);
        context.CityMunicipalities.Add(cityMunicipality);
        context.Barangays.Add(barangay);

        await context.SaveChangesAsync();

        var customer = new Customer(
            "Test Customer",
            "09170000000",
            "Infrastructure persistence test");

        var address = new CustomerAddress(
            customer.Id,
            "Home",
            "123 Test Street",
            barangay.Id,
            "1920",
            "Test Landmark",
            14.5631m,
            121.1324m,
            true);

        customer.AddAddress(address);

        // Act
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var savedCustomer =
            await context.Customers
                .Include(x => x.Addresses)
                    .ThenInclude(x => x.Barangay)
                        .ThenInclude(x => x.CityMunicipality)
                            .ThenInclude(x => x.Province!)
                                .ThenInclude(x => x.Region)
                .SingleAsync(x => x.Id == customer.Id);

        // Assert
        Assert.Equal(
            "Test Customer",
            savedCustomer.Name);

        Assert.Equal(
            "09170000000",
            savedCustomer.ContactNumber);

        Assert.Equal(
            "Infrastructure persistence test",
            savedCustomer.Notes);

        var savedAddress =
            Assert.Single(savedCustomer.Addresses);

        Assert.Equal(
            "Home",
            savedAddress.Label);

        Assert.Equal(
            "123 Test Street",
            savedAddress.Street);

        Assert.Equal(
            barangay.Id,
            savedAddress.BarangayId);

        Assert.Equal(
            "1920",
            savedAddress.PostalCode);

        Assert.Equal(
            "Test Landmark",
            savedAddress.Landmark);

        Assert.Equal(
            14.5631m,
            savedAddress.Latitude);

        Assert.Equal(
            121.1324m,
            savedAddress.Longitude);

        Assert.True(
            savedAddress.IsDefault);

        Assert.Equal(
            "Test Barangay",
            savedAddress.Barangay.Name);

        Assert.Equal(
            "Test City",
            savedAddress
                .Barangay
                .CityMunicipality
                .Name);

        Assert.Equal(
            "Test Province",
            savedAddress
                .Barangay
                .CityMunicipality
                .Province!
                .Name);

        Assert.Equal(
            "Test Region",
            savedAddress
                .Barangay
                .CityMunicipality
                .Province!
                .Region
                .Name);
    }
}