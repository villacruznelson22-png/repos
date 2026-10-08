using MediatR;

namespace WholesalePOS.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;