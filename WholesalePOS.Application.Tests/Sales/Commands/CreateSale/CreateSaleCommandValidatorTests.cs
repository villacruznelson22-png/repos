using FluentValidation.TestHelper;
using WholesalePOS.Application.Sales.Commands.CreateSale;

namespace WholesalePOS.Application.Tests.Sales.Commands.CreateSale;

public class CreateSaleCommandValidatorTests
{
    private readonly CreateSaleCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var command = CreateValidCommand(
            customerId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.CustomerId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerId_Is_Null()
    {
        var command = CreateValidCommand(
            customerId: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(
            x => x.CustomerId);
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
                new CreateSaleLineRequest(
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
                new CreateSaleLineRequest(
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
                new CreateSaleLineRequest(
                    Guid.NewGuid(),
                    -1,
                    180)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].Quantity");
    }

    [Fact]
    public void Should_Have_Error_When_UnitSellingPrice_Is_Negative()
    {
        var command = CreateValidCommand(
            lines:
            [
                new CreateSaleLineRequest(
                    Guid.NewGuid(),
                    10,
                    -1)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].UnitSellingPrice");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Duplicate_Products_Exist()
    {
        var productId = Guid.NewGuid();

        var command = CreateValidCommand(
            lines:
            [
                new CreateSaleLineRequest(
                    productId,
                    10,
                    180),

                new CreateSaleLineRequest(
                    productId,
                    20,
                    175)
            ]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateSaleCommand CreateValidCommand(
        Guid? customerId = null,
        IReadOnlyCollection<CreateSaleLineRequest>? lines = null)
    {
        return new CreateSaleCommand(
            customerId,
            DateTime.UtcNow,
            "SALE-001",
            "Test notes",
            lines ??
            [
                new CreateSaleLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);
    }
}