using FluentValidation;

namespace WholesalePOS.Application.Sales.Commands.CancelSale;

public sealed class CancelSaleValidator
    : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("Sale ID is required.");
    }
}