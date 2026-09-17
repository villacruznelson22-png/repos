using FluentValidation.TestHelper;
using WholesalePOS.Application.Suppliers.Commands.UpdateSupplier;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierCommandValidatorTests
{
    private readonly UpdateSupplierCommandValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenValid()
    {
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
            "JTI Philippines",
            "09170000000",
            "Manila",
            "Cigarette supplier");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenIdIsEmpty()
    {
        var command = new UpdateSupplierCommand(
            Guid.Empty,
            "JTI Philippines",
            null,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldFail_WhenNameIsEmpty()
    {
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
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
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
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
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
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
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
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
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
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
        var command = new UpdateSupplierCommand(
            Guid.NewGuid(),
            "JTI Philippines",
            null,
            null,
            new string('A', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}