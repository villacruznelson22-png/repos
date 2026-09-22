using FluentValidation;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.CreateDeliveryReceipt;

public class CreateDeliveryReceiptCommandValidator
    : AbstractValidator<CreateDeliveryReceiptCommand>
{
    public CreateDeliveryReceiptCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty();

        RuleFor(x => x.OccurredAt)
            .NotEmpty();

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.Lines)
            .NotEmpty();

        RuleForEach(x => x.Lines)
            .SetValidator(
                new CreateDeliveryReceiptLineRequestValidator());
    }
}

public class CreateDeliveryReceiptLineRequestValidator
    : AbstractValidator<CreateDeliveryReceiptLineRequest>
{
    public CreateDeliveryReceiptLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0);
    }
}