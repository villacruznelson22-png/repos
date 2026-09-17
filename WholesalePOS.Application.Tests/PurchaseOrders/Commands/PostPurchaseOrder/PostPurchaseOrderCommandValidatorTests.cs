using FluentValidation.TestHelper;
using WholesalePOS.Application.PurchaseOrders.Commands.PostPurchaseOrder;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.PostPurchaseOrder;

public class PostPurchaseOrderCommandValidatorTests
{
    private readonly PostPurchaseOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new PostPurchaseOrderCommand(
            Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var command = new PostPurchaseOrderCommand(
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}