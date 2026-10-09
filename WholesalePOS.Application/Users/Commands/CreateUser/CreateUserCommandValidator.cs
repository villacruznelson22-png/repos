using FluentValidation;

namespace WholesalePOS.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(128);
        RuleFor(x => x.Roles).NotNull().NotEmpty()
            .Must(roles => roles is not null && roles.All(role => !string.IsNullOrWhiteSpace(role)))
            .WithMessage("At least one valid role is required.");
        RuleForEach(x => x.Roles).MaximumLength(50);
    }
}