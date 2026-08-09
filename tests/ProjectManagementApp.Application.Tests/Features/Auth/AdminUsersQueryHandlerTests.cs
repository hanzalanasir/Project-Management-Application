using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.GetUserById;
using ProjectManagementApp.Application.Features.Auth.ListUsers;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class AdminUsersQueryHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Users:Paging:DefaultPageSize"] = "20",
            ["Users:Paging:MaxPageSize"] = "100",
        }).Build();

    [Fact]
    public async Task ListUsers_ReturnsEveryUser_Unscoped_IncludingDeactivated()
    {
        var activeUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "active@example.com", FullName = "Active", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var deactivatedUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "deactivated@example.com", FullName = "Deactivated", IsActive = false, CreatedAt = DateTimeOffset.UtcNow };

        var db = Substitute.For<IApplicationDbContext>();
        var mockUsers = new List<ApplicationUser> { activeUser, deactivatedUser }.BuildMockDbSet();
        db.Users.Returns(mockUsers);

        var userManager = CreateUserManager();
        userManager.GetRolesAsync(activeUser).Returns(new List<string> { "TeamMember" });
        userManager.GetRolesAsync(deactivatedUser).Returns(new List<string> { "ProjectManager" });

        var handler = new ListUsersQueryHandler(db, userManager, CreateConfiguration());
        var result = await handler.Handle(new ListUsersQuery(1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items.Should().Contain(u => u.Email == "deactivated@example.com" && !u.IsActive);
    }

    [Fact]
    public async Task GetUserById_UnknownId_ReturnsNotFound()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var mockUsers = new List<ApplicationUser>().BuildMockDbSet();
        db.Users.Returns(mockUsers);
        var userManager = CreateUserManager();

        var handler = new GetUserByIdQueryHandler(db, userManager);
        var result = await handler.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task GetUserById_KnownId_ReturnsDetailWithVersion()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com", FullName = "Dana", IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, Version = 7 };
        var db = Substitute.For<IApplicationDbContext>();
        var mockUsers = new List<ApplicationUser> { user }.BuildMockDbSet();
        db.Users.Returns(mockUsers);
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(user).Returns(new List<string> { "Admin" });

        var handler = new GetUserByIdQueryHandler(db, userManager);
        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be(7u);
        result.Value!.Role.Should().Be("Admin");
    }
}
