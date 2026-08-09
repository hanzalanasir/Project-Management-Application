using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Features.Auth.ListUsers;

// No scope predicate — Admin-only makes the role gate the entire authorization surface
// (spec US-001-07 7Cs); every user in the system is visible, including deactivated ones.
public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, Result<PagedResult<AdminUserSummary>>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly int _defaultPageSize;
    private readonly int _maxPageSize;

    public ListUsersQueryHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _defaultPageSize = configuration.GetValue("Users:Paging:DefaultPageSize", 20);
        _maxPageSize = configuration.GetValue("Users:Paging:MaxPageSize", 100);
    }

    public async Task<Result<PagedResult<AdminUserSummary>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? _defaultPageSize : Math.Min(request.PageSize, _maxPageSize);

        var query = _db.Users.AsNoTracking().OrderBy(u => u.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);

        var pageOfUsers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminUserSummary>(pageOfUsers.Count);
        foreach (var user in pageOfUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new AdminUserSummary(user.Id, user.FullName, user.Email!, roles.Single(), user.IsActive, user.CreatedAt));
        }

        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var result = new PagedResult<AdminUserSummary>(items, page, pageSize, totalCount, totalPages);
        return Result<PagedResult<AdminUserSummary>>.Success(result);
    }
}
