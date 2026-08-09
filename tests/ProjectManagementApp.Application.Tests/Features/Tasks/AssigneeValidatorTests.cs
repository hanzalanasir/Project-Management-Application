using FluentAssertions;
using ProjectManagementApp.Application.Features.Tasks.Common;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

// Reused by both CreateTask and ReassignTask (data-model.md, research R-6) — reads the shared
// team_members entity read-only, never calls a 004 handler and never writes it.
[Collection(PostgresCollection.Name)]
public class AssigneeValidatorTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public AssigneeValidatorTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task IsEligibleAsync_CandidateIsAnActiveTeamMemberOfTheProject_ReturnsTrue()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@assignee-validator-test.com").Build();
        var candidate = new ApplicationUserBuilder().WithEmail("candidate@assignee-validator-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, candidate);
        db.Projects.Add(project);
        db.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = candidate.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(CancellationToken.None);

        var validator = new AssigneeValidator(db);

        (await validator.IsEligibleAsync(project.Id, candidate.Id, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task IsEligibleAsync_CandidateNotInProjectPool_ReturnsFalse()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@assignee-validator-test.com").Build();
        var outsider = new ApplicationUserBuilder().WithEmail("outsider@assignee-validator-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, outsider);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var validator = new AssigneeValidator(db);

        (await validator.IsEligibleAsync(project.Id, outsider.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task IsEligibleAsync_CandidateInPoolButDeactivated_ReturnsFalse()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm3@assignee-validator-test.com").Build();
        var deactivated = new ApplicationUserBuilder().WithEmail("deactivated@assignee-validator-test.com").Inactive().Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(pm, deactivated);
        db.Projects.Add(project);
        db.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = deactivated.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(CancellationToken.None);

        var validator = new AssigneeValidator(db);

        (await validator.IsEligibleAsync(project.Id, deactivated.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task IsEligibleAsync_NullAssigneeId_ReturnsTrue_UnassignedIsLegal()
    {
        await using var db = _fixture.CreateDbContext();
        var validator = new AssigneeValidator(db);

        (await validator.IsEligibleAsync(Guid.NewGuid(), null, CancellationToken.None)).Should().BeTrue();
    }
}
