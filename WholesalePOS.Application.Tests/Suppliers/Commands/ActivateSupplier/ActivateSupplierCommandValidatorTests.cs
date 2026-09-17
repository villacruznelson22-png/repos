using FluentValidation.TestHelper;
using WholesalePOS.Application.Suppliers.Commands.ActivateSupplier;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.ActivateSupplier;

public class ActivateSupplierCommandValidatorTests
{
    private readonly ActivateSupplierCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new ActivateSupplierCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var command = new ActivateSupplierCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}