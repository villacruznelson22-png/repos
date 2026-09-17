using FluentValidation.TestHelper;
using WholesalePOS.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.UpdatePurchaseOrder;

public class UpdatePurchaseOrderCommandValidatorTests
{
    private readonly UpdatePurchaseOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = CreateValidCommand(
            id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_Lines_Are_Empty()
    {
        var command = CreateValidCommand(
            lines: []);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Lines);
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Empty()
    {
        var command = CreateValidCommand(
            lines:
            [
                new UpdatePurchaseOrderLineRequest(
                    Guid.Empty,
                    10,
                    180)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].ProductId");
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Invalid()
    {
        var command = CreateValidCommand(
            lines:
            [
                new UpdatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    0,
                    180)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].Quantity");
    }

    [Fact]
    public void Should_Have_Error_When_UnitCost_Is_Negative()
    {
        var command = CreateValidCommand(
            lines:
            [
                new UpdatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    -1)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].UnitCost");
    }

    [Fact]
    public void Should_Have_Error_When_Duplicate_Products_Exist()
    {
        var productId = Guid.NewGuid();

        var command = CreateValidCommand(
            lines:
            [
                new UpdatePurchaseOrderLineRequest(
                    productId,
                    10,
                    180),

                new UpdatePurchaseOrderLineRequest(
                    productId,
                    20,
                    180)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Lines);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static UpdatePurchaseOrderCommand CreateValidCommand(
        Guid? id = null,
        IReadOnlyCollection<UpdatePurchaseOrderLineRequest>? lines = null)
    {
        return new UpdatePurchaseOrderCommand(
            id ?? Guid.NewGuid(),
            DateTime.UtcNow,
            "PO-001",
            "Test notes",
            lines ??
            [
                new UpdatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);
    }
}