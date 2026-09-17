using FluentValidation.TestHelper;
using WholesalePOS.Application.Customers.Commands.CreateCustomer;
using WholesalePOS.Application.Customers.DTOs;
using WholesalePOS.Application.Customers.Validators;

namespace WholesalePOS.Application.Tests.Customers.Validators;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();

    [Fact]
    public void ShouldBeValid_WhenCustomerHasValidInformation()
    {
        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer",
            null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldBeValid_WhenCustomerHasValidAddress()
    {
        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer",
            new CustomerAddressDto(
                "Home",
                "123 Mabini Street",
                Guid.NewGuid(),
                "1920",
                "Near the church",
                14.5631m,
                121.1324m,
                true));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenCustomerNameIsEmpty()
    {
        var command = new CreateCustomerCommand(
            "",
            null,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenCustomerNameExceedsMaximumLength()
    {
        var command = new CreateCustomerCommand(
            new string('A', 201),
            null,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenContactNumberExceedsMaximumLength()
    {
        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            new string('1', 51),
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContactNumber);
    }

    [Fact]
    public void ShouldHaveError_WhenNotesExceedMaximumLength()
    {
        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            null,
            new string('A', 1001),
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void ShouldHaveError_WhenAddressLabelIsEmpty()
    {
        var command = CreateCommandWithAddress(
            label: "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Address.Label");
    }

    [Fact]
    public void ShouldHaveError_WhenBarangayIdIsEmpty()
    {
        var command = CreateCommandWithAddress(
            barangayId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Address.BarangayId");
    }

    [Fact]
    public void ShouldHaveError_WhenLatitudeIsBelowMinimum()
    {
        var command = CreateCommandWithAddress(
            latitude: -91m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Address.Latitude");
    }

    [Fact]
    public void ShouldHaveError_WhenLatitudeIsAboveMaximum()
    {
        var command = CreateCommandWithAddress(
            latitude: 91m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Address.Latitude");
    }

    [Fact]
    public void ShouldHaveError_WhenLongitudeIsBelowMinimum()
    {
        var command = CreateCommandWithAddress(
            longitude: -181m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Address.Longitude");
    }

    [Fact]
    public void ShouldHaveError_WhenLongitudeIsAboveMaximum()
    {
        var command = CreateCommandWithAddress(
            longitude: 181m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Address.Longitude");
    }

    private static CreateCustomerCommand CreateCommandWithAddress(
        string label = "Home",
        string? street = "123 Mabini Street",
        Guid? barangayId = null,
        string? postalCode = "1920",
        string? landmark = "Near the church",
        decimal? latitude = 14.5631m,
        decimal? longitude = 121.1324m,
        bool isDefault = true)
    {
        var address = new CustomerAddressDto(
            label,
            street,
            barangayId ?? Guid.NewGuid(),
            postalCode,
            landmark,
            latitude,
            longitude,
            isDefault);

        return new CreateCustomerCommand(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer",
            address);
    }
}