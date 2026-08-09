using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Projects.UpdateProject;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

// FR-010: CanMutateAsync is re-evaluated at write time, never trusted from an earlier read. Proven
// here by transferring ownership away from the caller mid-scenario, then attempting a second write
// with the caller's ORIGINAL (now stale) role-in-scope assumption still "true" from their point of
// view — the handler must deny it because it re-checks against current DB state, not a cached read.
[Collection(PostgresCollection.Name)]
public class UpdateProjectWriteTimeCheckTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public UpdateProjectWriteTimeCheckTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_OwnershipChangedSinceTheCallerLastRead_DeniesTheStaleCallersMutation()
    {
        Guid projectId;
        uint versionAtRead;
        ApplicationUser originalOwner;
        ApplicationUser newOwner;

        await using (var seedDb = _fixture.CreateDbContext())
        {
            originalOwner = new ApplicationUserBuilder().WithEmail("original-owner@writetime-test.com").Build();
            newOwner = new ApplicationUserBuilder().WithEmail("new-owner@writetime-test.com").Build();
            var project = new ProjectBuilder().WithOwner(originalOwner).Build();
            seedDb.Users.AddRange(originalOwner, newOwner);
            seedDb.Projects.Add(project);
            await seedDb.SaveChangesAsync(CancellationToken.None);
            projectId = project.Id;
            versionAtRead = project.Version;
        }

        // Ownership transfers away from originalOwner via a SEPARATE, untracked context — simulating
        // another actor's write landing between the caller's read and their own write attempt.
        await using (var transferDb = _fixture.CreateDbContext())
        {
            var tracked = await transferDb.Projects.FindAsync(projectId);
            tracked!.OwnerId = newOwner.Id;
            await transferDb.SaveChangesAsync(CancellationToken.None);
        }

        // A FRESH, untracked context for the handler itself — reusing seedDb would hand the handler
        // its own stale in-memory copy of `project` (EF's identity-map shortcut returns the tracked
        // instance instead of re-reading the row), silently defeating the very re-check under test.
        await using var db = _fixture.CreateDbContext();
        var accessPolicy = new ProjectAccessPolicy(db);
        var userManager = CreateUserManager();
        var activityLog = Substitute.For<IActivityLogService>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(originalOwner.Id, originalOwner.Email!, "ProjectManager", originalOwner.FullName));

        var handler = new UpdateProjectCommandHandler(db, accessPolicy, userManager, currentUser, activityLog, Options.Create(new ProjectsOptions()));
        var command = new UpdateProjectCommand(projectId, "Attempted After Transfer", null, DateOnly.FromDateTime(DateTime.UtcNow), null, "Active", null, versionAtRead);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
    }
}
