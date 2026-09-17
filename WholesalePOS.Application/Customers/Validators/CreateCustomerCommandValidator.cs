using FluentValidation;
using WholesalePOS.Application.Customers.Commands.CreateCustomer;

namespace WholesalePOS.Application.Customers.Validators;

public class CreateCustomerCommandValidator: AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(50);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.Label)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Address!.Street)
                .MaximumLength(300);

            RuleFor(x => x.Address!.BarangayId)
                .NotEmpty();

            RuleFor(x => x.Address!.PostalCode)
                .MaximumLength(20);

            RuleFor(x => x.Address!.Landmark)
                .MaximumLength(300);

            RuleFor(x => x.Address!.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Address!.Latitude.HasValue);

            RuleFor(x => x.Address!.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Address!.Longitude.HasValue);
        });
    }
}