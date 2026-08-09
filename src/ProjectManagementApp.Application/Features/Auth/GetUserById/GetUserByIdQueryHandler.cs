using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Features.Auth.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<AdminUserDetail>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result<AdminUserDetail>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.NotFound, "User not found"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new AdminUserDetail(
            user.Id, user.FullName, user.Email!, roles.Single(), user.IsActive,
            user.CreatedAt, user.UpdatedAt, user.Version);

        return Result<AdminUserDetail>.Success(dto);
    }
}
