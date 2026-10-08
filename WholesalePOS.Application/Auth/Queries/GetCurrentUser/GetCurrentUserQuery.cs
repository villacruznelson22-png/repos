using MediatR;
using WholesalePOS.Application.Auth.DTOs;

namespace WholesalePOS.Application.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserSummaryDto>;