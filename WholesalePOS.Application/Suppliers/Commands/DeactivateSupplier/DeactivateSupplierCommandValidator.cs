using FluentValidation;

namespace WholesalePOS.Application.Suppliers.Commands.DeactivateSupplier;

public class DeactivateSupplierCommandValidator
    : AbstractValidator<DeactivateSupplierCommand>
{
    public DeactivateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}