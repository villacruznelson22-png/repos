using FluentValidation;

namespace WholesalePOS.Application.Users.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(12).MaximumLength(128);
    }
}