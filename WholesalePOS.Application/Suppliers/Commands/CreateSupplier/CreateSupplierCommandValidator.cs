using FluentValidation;

namespace WholesalePOS.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandValidator
    : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(50);

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}