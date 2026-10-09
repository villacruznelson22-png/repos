using MediatR;

namespace WholesalePOS.Application.Users.Commands.SetUserActive;

public sealed record SetUserActiveCommand(Guid Id, bool IsActive) : IRequest;