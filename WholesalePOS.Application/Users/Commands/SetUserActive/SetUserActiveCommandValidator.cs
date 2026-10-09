using FluentValidation;

namespace WholesalePOS.Application.Users.Commands.SetUserActive;

public sealed class SetUserActiveCommandValidator : AbstractValidator<SetUserActiveCommand>
{
    public SetUserActiveCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}