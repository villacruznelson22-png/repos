using FluentValidation;

namespace WholesalePOS.Application.Sales.Commands.CheckoutSale;

public sealed class CheckoutSaleCommandValidator
    : AbstractValidator<CheckoutSaleCommand>
{
    public CheckoutSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(100);

        RuleForEach(x => x.Payments)
            .ChildRules(payment =>
            {
                payment.RuleFor(x => x.Amount)
                    .GreaterThan(0);

                payment.RuleFor(x => x.IdempotencyKey)
                    .NotEmpty()
                    .MaximumLength(100);

                payment.RuleFor(x => x.ReferenceNumber)
                    .MaximumLength(100);
            });
    }
}
