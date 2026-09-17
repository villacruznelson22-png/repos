using FluentValidation;

namespace WholesalePOS.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public class UpdatePurchaseOrderCommandValidator
    : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.OccurredAt)
            .NotEmpty();

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("Purchase order must contain at least one line.");

        RuleForEach(x => x.Lines)
            .SetValidator(new UpdatePurchaseOrderLineRequestValidator());

        RuleFor(x => x.Lines)
            .Must(HaveUniqueProducts)
            .WithMessage(
                "The same product cannot appear more than once in a purchase order.");
    }

    private static bool HaveUniqueProducts(
        IReadOnlyCollection<UpdatePurchaseOrderLineRequest> lines)
    {
        return lines
            .Select(x => x.ProductId)
            .Distinct()
            .Count() == lines.Count;
    }
}

public class UpdatePurchaseOrderLineRequestValidator
    : AbstractValidator<UpdatePurchaseOrderLineRequest>
{
    public UpdatePurchaseOrderLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0);
    }
}