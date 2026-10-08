using FluentValidation.TestHelper;
using WholesalePOS.Application.Sales.Commands.UpdateSale;

namespace WholesalePOS.Application.Tests.Sales.Commands.UpdateSale;

public class UpdateSaleCommandValidatorTests
{
    private readonly UpdateSaleCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_SaleId_Is_Empty()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(saleId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.SaleId);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(customerId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerId_Is_Null()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(customerId: null));

        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Have_Error_When_Lines_Are_Empty()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(lines: []));

        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Empty()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(
                lines:
                [
                    new UpdateSaleLineRequest(
                        Guid.Empty,
                        1,
                        100)
                ]));

        result.ShouldHaveValidationErrorFor(
            "Lines[0].ProductId");
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Not_Positive()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(
                lines:
                [
                    new UpdateSaleLineRequest(
                        Guid.NewGuid(),
                        0,
                        100)
                ]));

        result.ShouldHaveValidationErrorFor(
            "Lines[0].Quantity");
    }

    [Fact]
    public void Should_Have_Error_When_UnitSellingPrice_Is_Negative()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(
                lines:
                [
                    new UpdateSaleLineRequest(
                        Guid.NewGuid(),
                        1,
                        -1)
                ]));

        result.ShouldHaveValidationErrorFor(
            "Lines[0].UnitSellingPrice");
    }

    [Fact]
    public void Should_Have_Error_When_Duplicate_Products_Exist()
    {
        var productId = Guid.NewGuid();

        var result = _validator.TestValidate(
            CreateValidCommand(
                lines:
                [
                    new UpdateSaleLineRequest(productId, 1, 100),
                    new UpdateSaleLineRequest(productId, 2, 90)
                ]));

        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Should_Have_Error_When_ReferenceNumber_Is_Too_Long()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(
                referenceNumber: new string('R', 101)));

        result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber);
    }

    [Fact]
    public void Should_Have_Error_When_Notes_Are_Too_Long()
    {
        var result = _validator.TestValidate(
            CreateValidCommand(
                notes: new string('N', 1001)));

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var result = _validator.TestValidate(CreateValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static UpdateSaleCommand CreateValidCommand(
        Guid? saleId = null,
        Guid? customerId = null,
        string? referenceNumber = "SALE-001",
        string? notes = "Test notes",
        IReadOnlyCollection<UpdateSaleLineRequest>? lines = null)
    {
        return new UpdateSaleCommand(
            saleId ?? Guid.NewGuid(),
            customerId,
            DateTime.UtcNow,
            referenceNumber,
            notes,
            lines ??
            [
                new UpdateSaleLineRequest(
                    Guid.NewGuid(),
                    1,
                    100)
            ]);
    }
}
