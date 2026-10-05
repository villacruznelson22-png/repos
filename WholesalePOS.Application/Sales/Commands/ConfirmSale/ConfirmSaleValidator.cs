using FluentValidation;

namespace WholesalePOS.Application.Sales.Commands.ConfirmSale;

public sealed class ConfirmSaleValidator
    : AbstractValidator<ConfirmSaleCommand>
{
    public ConfirmSaleValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("Sale ID is required.");
    }
}