using MediatR;
using WholesalePOS.Application.Auth.DTOs;

namespace WholesalePOS.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<AuthResponse>;