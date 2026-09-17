using FluentValidation.TestHelper;
using WholesalePOS.Application.Suppliers.Commands.DeactivateSupplier;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.DeactivateSupplier;

public class DeactivateSupplierCommandValidatorTests
{
    private readonly DeactivateSupplierCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new DeactivateSupplierCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var command = new DeactivateSupplierCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}