using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Auth.Commands.Login;
using WholesalePOS.Application.Auth.Commands.Logout;
using WholesalePOS.Application.Auth.Commands.Refresh;
using WholesalePOS.Application.Auth.DTOs;
using WholesalePOS.Application.Auth.Queries.GetCurrentUser;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    public AuthController(ISender sender) => _sender = sender;

    [AllowAnonymous, HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        => Ok(await _sender.Send(new LoginCommand(request.Username, request.Password), ct));

    [AllowAnonymous, HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct)
        => Ok(await _sender.Send(new RefreshCommand(request.RefreshToken), ct));

    [AllowAnonymous, HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await _sender.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<UserSummaryDto>> Me(CancellationToken ct)
        => Ok(await _sender.Send(new GetCurrentUserQuery(), ct));
}