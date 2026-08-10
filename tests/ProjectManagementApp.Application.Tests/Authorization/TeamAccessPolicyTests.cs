using FluentAssertions;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Authorization;

// Covers all six role x operation cells from data-model.md §3's view-vs-manage table, plus the
// cell the whole feature turns on: a ProjectManager who is a MEMBER but not the OWNER of a
// project must pass CanViewTeamAsync and fail CanManageTeamAsync. That divergence is why the two
// methods cannot be collapsed into one (research R-1).
[Collection(PostgresCollection.Name)]
public class TeamAccessPolicyTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public TeamAccessPolicyTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser Caller(Guid userId, Role role) =>
        new(userId, "caller@team-policy-test.com", role.ToString(), "Caller");

    [Fact]
    public async Task CanViewTeamAsync_Admin_AlwaysAllowed()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner1@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanViewTeamAsync(project, Caller(Guid.NewGuid(), Role.Admin), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewTeamAsync_ProjectManager_Owner_Allowed()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner2@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanViewTeamAsync(project, Caller(owner.Id, Role.ProjectManager), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewTeamAsync_ProjectManager_MemberButNotOwner_Allowed()
    {
        // The real cell: a PM can be a member of a project they do not own — any active user is
        // eligible regardless of global role (Clarifications 2026-07-22).
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner3@team-policy-test.com").Build();
        var memberPm = new ApplicationUserBuilder().WithEmail("member-pm@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, memberPm);
        db.Projects.Add(project);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = memberPm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanViewTeamAsync(project, Caller(memberPm.Id, Role.ProjectManager), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewTeamAsync_ProjectManager_NeitherOwnerNorMember_Denied()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner4@team-policy-test.com").Build();
        var strangerPm = new ApplicationUserBuilder().WithEmail("stranger-pm@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, strangerPm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanViewTeamAsync(project, Caller(strangerPm.Id, Role.ProjectManager), CancellationToken.None);

        decision.Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task CanViewTeamAsync_TeamMember_Member_Allowed()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner5@team-policy-test.com").Build();
        var memberTm = new ApplicationUserBuilder().WithEmail("member-tm@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, memberTm);
        db.Projects.Add(project);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = memberTm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanViewTeamAsync(project, Caller(memberTm.Id, Role.TeamMember), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanViewTeamAsync_TeamMember_NotMember_Denied()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner6@team-policy-test.com").Build();
        var strangerTm = new ApplicationUserBuilder().WithEmail("stranger-tm@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, strangerTm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanViewTeamAsync(project, Caller(strangerTm.Id, Role.TeamMember), CancellationToken.None);

        decision.Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task CanManageTeamAsync_Admin_AlwaysAllowed()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner7@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanManageTeamAsync(project, Caller(Guid.NewGuid(), Role.Admin), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanManageTeamAsync_ProjectManager_Owner_Allowed()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner8@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanManageTeamAsync(project, Caller(owner.Id, Role.ProjectManager), CancellationToken.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task CanManageTeamAsync_ProjectManager_MemberButNotOwner_Denied()
    {
        // The divergent cell: this same PM passed CanViewTeamAsync above but must fail here —
        // membership grants visibility, never management authority (research R-1).
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner9@team-policy-test.com").Build();
        var memberPm = new ApplicationUserBuilder().WithEmail("member-pm2@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, memberPm);
        db.Projects.Add(project);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = memberPm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);

        (await policy.CanViewTeamAsync(project, Caller(memberPm.Id, Role.ProjectManager), CancellationToken.None)).Allowed.Should().BeTrue();
        (await policy.CanManageTeamAsync(project, Caller(memberPm.Id, Role.ProjectManager), CancellationToken.None)).Allowed.Should().BeFalse();
    }

    [Fact]
    public async Task CanManageTeamAsync_TeamMember_AlwaysDenied_EvenIfMember()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner10@team-policy-test.com").Build();
        var memberTm = new ApplicationUserBuilder().WithEmail("member-tm2@team-policy-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, memberTm);
        db.Projects.Add(project);
        db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = memberTm.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TeamAccessPolicy(db);
        var decision = await policy.CanManageTeamAsync(project, Caller(memberTm.Id, Role.TeamMember), CancellationToken.None);

        decision.Allowed.Should().BeFalse();
    }
}
