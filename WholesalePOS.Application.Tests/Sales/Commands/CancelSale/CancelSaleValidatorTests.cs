using FluentValidation.TestHelper;
using WholesalePOS.Application.Sales.Commands.CancelSale;

namespace WholesalePOS.Application.Tests.Sales.Commands.CancelSale;

public class CancelSaleValidatorTests
{
    private readonly CancelSaleValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_SaleId_Is_Empty()
    {
        var result = _validator.TestValidate(
            new CancelSaleCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.SaleId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_SaleId_Is_Valid()
    {
        var result = _validator.TestValidate(
            new CancelSaleCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
