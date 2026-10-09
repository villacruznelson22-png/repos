using MediatR;

namespace WholesalePOS.Application.Users.Commands.ChangeUserPassword;

public sealed record ChangeUserPasswordCommand(Guid Id, string NewPassword) : IRequest;