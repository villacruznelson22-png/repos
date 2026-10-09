using MediatR;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Users.DTOs;

namespace WholesalePOS.Application.Users.Queries.GetUsers;

public sealed class GetUsersQuery : PaginationRequest, IRequest<PagedResult<UserListItemDto>>
{
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
}