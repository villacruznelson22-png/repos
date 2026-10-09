using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Authorization;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Users.Commands.ChangeUserPassword;
using WholesalePOS.Application.Users.Commands.CreateUser;
using WholesalePOS.Application.Users.Commands.SetUserActive;
using WholesalePOS.Application.Users.Commands.UpdateUser;
using WholesalePOS.Application.Users.DTOs;
using WholesalePOS.Application.Users.Queries.GetUserById;
using WholesalePOS.Application.Users.Queries.GetUsers;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;
    public UsersController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListItemDto>>> Get([FromQuery] GetUsersQuery query, CancellationToken ct)
        => Ok(await _sender.Send(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailsDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetUserByIdQuery(id), ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateUserCommand(id, request.Username, request.DisplayName, request.Roles), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetUserActiveRequest request, CancellationToken ct)
    {
        await _sender.Send(new SetUserActiveCommand(id, request.IsActive), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangeUserPasswordRequest request, CancellationToken ct)
    {
        await _sender.Send(new ChangeUserPasswordCommand(id, request.NewPassword), ct);
        return NoContent();
    }
}

public sealed record UpdateUserRequest(string Username, string DisplayName, IReadOnlyCollection<string> Roles);
public sealed record SetUserActiveRequest(bool IsActive);
public sealed record ChangeUserPasswordRequest(string NewPassword);
