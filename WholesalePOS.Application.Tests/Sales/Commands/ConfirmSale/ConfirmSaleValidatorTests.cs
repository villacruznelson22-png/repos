using FluentValidation.TestHelper;
using WholesalePOS.Application.Sales.Commands.ConfirmSale;

namespace WholesalePOS.Application.Tests.Sales.Commands.ConfirmSale;

public class ConfirmSaleValidatorTests
{
    private readonly ConfirmSaleValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_SaleId_Is_Empty()
    {
        var result = _validator.TestValidate(
            new ConfirmSaleCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.SaleId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_SaleId_Is_Valid()
    {
        var result = _validator.TestValidate(
            new ConfirmSaleCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
