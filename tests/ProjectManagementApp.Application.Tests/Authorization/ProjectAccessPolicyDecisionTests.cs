using FluentAssertions;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Authorization;

// Covers all six role x operation cells from data-model.md §3's decision table.
[Collection(PostgresCollection.Name)]
public class ProjectAccessPolicyDecisionTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ProjectAccessPolicyDecisionTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser Caller(Guid userId, Role role) =>
        new(userId, "caller@decision-test.com", role.ToString(), "Caller");

    [Fact]
    public async Task CanReadAsync_Admin_AlwaysAllowed()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@decision-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);
        var decision = await policy.CanReadAsync(project, Caller(Guid.NewGuid(), Role.Admin), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanReadAsync_ProjectManager_AllowedOnlyIfOwner()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("owner@decision-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other-owner@decision-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, otherPm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);

        (await policy.CanReadAsync(project, Caller(pm.Id, Role.ProjectManager), CancellationToken.None)).Allowed.Should().BeTrue();
        (await policy.CanReadAsync(project, Caller(otherPm.Id, Role.ProjectManager), CancellationToken.None)).Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task CanReadAsync_TeamMember_AllowedOnlyIfAssigned()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@decision-test.com").Build();
        var assignedTm = new ApplicationUserBuilder().WithEmail("assigned@decision-test.com").Build();
        var unassignedTm = new ApplicationUserBuilder().WithEmail("unassigned@decision-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, assignedTm, unassignedTm);
        db.Projects.Add(project);
        db.TeamMembers.Add(new Domain.Entities.TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = assignedTm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);

        (await policy.CanReadAsync(project, Caller(assignedTm.Id, Role.TeamMember), CancellationToken.None)).Allowed.Should().BeTrue();
        (await policy.CanReadAsync(project, Caller(unassignedTm.Id, Role.TeamMember), CancellationToken.None)).Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task CanMutateAsync_Admin_AlwaysAllowed()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm3@decision-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);
        var decision = await policy.CanMutateAsync(project, Caller(Guid.NewGuid(), Role.Admin), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanMutateAsync_ProjectManager_AllowedOnlyIfOwner()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("owner2@decision-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other-owner2@decision-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, otherPm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);

        (await policy.CanMutateAsync(project, Caller(pm.Id, Role.ProjectManager), CancellationToken.None)).Allowed.Should().BeTrue();
        (await policy.CanMutateAsync(project, Caller(otherPm.Id, Role.ProjectManager), CancellationToken.None)).Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task CanMutateAsync_TeamMember_AlwaysDenied_EvenIfAssigned()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm4@decision-test.com").Build();
        var assignedTm = new ApplicationUserBuilder().WithEmail("assigned2@decision-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, assignedTm);
        db.Projects.Add(project);
        db.TeamMembers.Add(new Domain.Entities.TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = assignedTm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new ProjectAccessPolicy(db);
        var decision = await policy.CanMutateAsync(project, Caller(assignedTm.Id, Role.TeamMember), CancellationToken.None);

        decision.Allowed.Should().BeFalse();
    }
}
