using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Auth.ListUsers;

public sealed record ListUsersQuery(int Page, int PageSize) : IRequest<Result<PagedResult<AdminUserSummary>>>;
