using FluentValidation.TestHelper;
using WholesalePOS.Application.DeliveryReceipts.Commands.CreateDeliveryReceipt;

namespace WholesalePOS.Application.Tests.DeliveryReceipts.Commands.CreateDeliveryReceipt;

public class CreateDeliveryReceiptCommandValidatorTests
{
    [Fact]
    public void Should_Pass_When_CommandIsValid()
    {
        var command = CreateValidCommand();

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_PurchaseOrderIdIsEmpty()
    {
        var command = CreateValidCommand() with
        {
            PurchaseOrderId = Guid.Empty
        };

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PurchaseOrderId);
    }

    [Fact]
    public void Should_Fail_When_OccurredAtIsDefault()
    {
        var command = CreateValidCommand() with
        {
            OccurredAt = default
        };

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OccurredAt);
    }

    [Fact]
    public void Should_Fail_When_LinesAreEmpty()
    {
        var command = CreateValidCommand() with
        {
            Lines = Array.Empty<CreateDeliveryReceiptLineRequest>()
        };

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Should_Fail_When_ProductIdIsEmpty()
    {
        var command = CreateValidCommand(
            new CreateDeliveryReceiptLineRequest(
                Guid.Empty,
                10,
                180,
                null));

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].ProductId");
    }

    [Fact]
    public void Should_Fail_When_QuantityIsZero()
    {
        var command = CreateValidCommand(
            new CreateDeliveryReceiptLineRequest(
                Guid.NewGuid(),
                0,
                180,
                null));

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].Quantity");
    }

    [Fact]
    public void Should_Fail_When_QuantityIsNegative()
    {
        var command = CreateValidCommand(
            new CreateDeliveryReceiptLineRequest(
                Guid.NewGuid(),
                -1,
                180,
                null));

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].Quantity");
    }

    [Fact]
    public void Should_Fail_When_UnitCostIsNegative()
    {
        var command = CreateValidCommand(
            new CreateDeliveryReceiptLineRequest(
                Guid.NewGuid(),
                10,
                -1,
                null));

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            "Lines[0].UnitCost");
    }

    [Fact]
    public void Should_Pass_When_UnitCostIsZero()
    {
        var command = CreateValidCommand(
            new CreateDeliveryReceiptLineRequest(
                Guid.NewGuid(),
                10,
                0,
                null));

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_ExpirationDateIsNull()
    {
        var command = CreateValidCommand(
            new CreateDeliveryReceiptLineRequest(
                Guid.NewGuid(),
                10,
                180,
                null));

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_ReferenceNumberIsTooLong()
    {
        var command = CreateValidCommand() with
        {
            ReferenceNumber = new string('A', 101)
        };

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber);
    }

    [Fact]
    public void Should_Fail_When_NotesIsTooLong()
    {
        var command = CreateValidCommand() with
        {
            Notes = new string('A', 1001)
        };

        var validator = new CreateDeliveryReceiptCommandValidator();

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    private static CreateDeliveryReceiptCommand CreateValidCommand(
        CreateDeliveryReceiptLineRequest? line = null)
    {
        return new CreateDeliveryReceiptCommand(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "DR-001",
            "Test delivery",
            new[]
            {
                line ?? new CreateDeliveryReceiptLineRequest(
                    Guid.NewGuid(),
                    10,
                    180,
                    null)
            });
    }
}