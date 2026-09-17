using FluentValidation.TestHelper;
using WholesalePOS.Application.Suppliers.Commands.CreateSupplier;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandValidatorTests
{
    private readonly CreateSupplierCommandValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenValid()
    {
        var command = new CreateSupplierCommand(
            "JTI Philippines",
            "09170000000",
            "Manila",
            "Cigarette supplier");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateSupplierCommand(
            "",
            null,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldFail_WhenNameExceedsMaximumLength()
    {
        var command = new CreateSupplierCommand(
            new string('A', 201),
            null,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldPass_WhenOptionalFieldsAreNull()
    {
        var command = new CreateSupplierCommand(
            "JTI Philippines",
            null,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenContactNumberExceedsMaximumLength()
    {
        var command = new CreateSupplierCommand(
            "JTI Philippines",
            new string('1', 51),
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContactNumber);
    }

    [Fact]
    public void ShouldFail_WhenAddressExceedsMaximumLength()
    {
        var command = new CreateSupplierCommand(
            "JTI Philippines",
            null,
            new string('A', 501),
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void ShouldFail_WhenNotesExceedMaximumLength()
    {
        var command = new CreateSupplierCommand(
            "JTI Philippines",
            null,
            null,
            new string('A', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}