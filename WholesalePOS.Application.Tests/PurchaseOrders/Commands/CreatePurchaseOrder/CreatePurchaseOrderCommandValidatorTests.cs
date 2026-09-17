using FluentValidation.TestHelper;
using WholesalePOS.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandValidatorTests
{
    private readonly CreatePurchaseOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_SupplierId_Is_Empty()
    {
        var command = CreateValidCommand(
            supplierId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.SupplierId);
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
                new CreatePurchaseOrderLineRequest(
                    Guid.Empty,
                    10,
                    180)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].ProductId");
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Zero()
    {
        var command = CreateValidCommand(
            lines:
            [
                new CreatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    0,
                    180)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].Quantity");
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Negative()
    {
        var command = CreateValidCommand(
            lines:
            [
                new CreatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    -1,
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
                new CreatePurchaseOrderLineRequest(
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
                new CreatePurchaseOrderLineRequest(
                    productId,
                    10,
                    180),

                new CreatePurchaseOrderLineRequest(
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

    private static CreatePurchaseOrderCommand CreateValidCommand(
        Guid? supplierId = null,
        IReadOnlyCollection<CreatePurchaseOrderLineRequest>? lines = null)
    {
        return new CreatePurchaseOrderCommand(
            supplierId ?? Guid.NewGuid(),
            DateTime.UtcNow,
            "PO-001",
            "Test notes",
            lines ??
            [
                new CreatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);
    }
}