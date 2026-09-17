using FluentValidation.TestHelper;
using WholesalePOS.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderCommandValidatorTests
{
    private readonly CancelPurchaseOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new CancelPurchaseOrderCommand(
            Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var command = new CancelPurchaseOrderCommand(
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}