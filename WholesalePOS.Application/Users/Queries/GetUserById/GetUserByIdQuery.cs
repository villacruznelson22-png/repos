using MediatR;
using WholesalePOS.Application.Users.DTOs;

namespace WholesalePOS.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDetailsDto>;