using FluentValidation;

namespace WholesalePOS.Application.Suppliers.Commands.ActivateSupplier;

public class ActivateSupplierCommandValidator
    : AbstractValidator<ActivateSupplierCommand>
{
    public ActivateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}