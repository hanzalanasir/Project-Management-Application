using MediatR;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Register;

namespace ProjectManagementApp.Application.Features.Auth.GetCurrentUser;

// Projects UserDto directly from ICurrentUserService's token-derived claims — never from
// IApplicationDbContext. Zero database round-trips (NFR-002, quickstart, T082 verifies this).
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var current = _currentUserService.Current;
        var dto = new UserDto(current.UserId, current.FullName, current.Email, current.Role, CreatedAt: null);
        return Task.FromResult(Result<UserDto>.Success(dto));
    }
}
