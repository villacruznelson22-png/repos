using MediatR;

namespace WholesalePOS.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(string Username, string DisplayName, string Password, IReadOnlyCollection<string> Roles) : IRequest<Guid>;