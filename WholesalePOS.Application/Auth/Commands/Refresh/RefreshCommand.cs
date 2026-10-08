using MediatR;
using WholesalePOS.Application.Auth.DTOs;

namespace WholesalePOS.Application.Auth.Commands.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;