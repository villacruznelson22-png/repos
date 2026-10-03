using FluentValidation;

namespace WholesalePOS.Application.Sales.Commands.CreateSale;

public class CreateSaleCommandValidator
    : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
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
            .SetValidator(
                new CreateSaleLineRequestValidator());
    }
}

public class CreateSaleLineRequestValidator
    : AbstractValidator<CreateSaleLineRequest>
{
    public CreateSaleLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitSellingPrice)
            .GreaterThanOrEqualTo(0);
    }
}