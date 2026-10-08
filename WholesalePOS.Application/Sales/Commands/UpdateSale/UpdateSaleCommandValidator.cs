using FluentValidation;

namespace WholesalePOS.Application.Sales.Commands.UpdateSale;

public sealed class UpdateSaleCommandValidator
    : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .Must(x => x == null || x != Guid.Empty)
            .WithMessage("Customer ID cannot be empty.");

        RuleFor(x => x.OccurredAt)
            .NotEmpty();

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.Lines)
            .NotEmpty();

        RuleForEach(x => x.Lines)
            .SetValidator(new UpdateSaleLineRequestValidator());

        RuleFor(x => x.Lines)
            .Must(lines => lines
                .Select(x => x.ProductId)
                .Distinct()
                .Count() == lines.Count)
            .WithMessage("The same product cannot appear more than once in a sale.");
    }
}

public sealed class UpdateSaleLineRequestValidator
    : AbstractValidator<UpdateSaleLineRequest>
{
    public UpdateSaleLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitSellingPrice)
            .GreaterThanOrEqualTo(0);
    }
}
