using MediatR;

namespace WholesalePOS.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid Id, string Username, string DisplayName, IReadOnlyCollection<string> Roles) : IRequest;