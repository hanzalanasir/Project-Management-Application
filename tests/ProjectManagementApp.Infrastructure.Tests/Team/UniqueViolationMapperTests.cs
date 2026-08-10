using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Team;

// A 23505 on team_members' unique index must be tested against real PostgreSQL — the SQLSTATE and
// constraint name are provider behaviour, unprovable on EF InMemory (research R-3, ADR-0007 §2).
[Collection(PostgresCollection.Name)]
public class UniqueViolationMapperTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public UniqueViolationMapperTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static ApplicationUser NewUser(string email) => new()
    {
        Id = Guid.NewGuid(),
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        FullName = email,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    private static Project NewProject(string name, ApplicationUser owner) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Owner = owner,
        OwnerId = owner.Id,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task DuplicateMembership_ThrowsDbUpdateException_MappedAsUniqueViolation()
    {
        var owner = NewUser("uvm-owner@example.com");
        var member = NewUser("uvm-member@example.com");
        var project = NewProject("UVM Project", owner);

        await using (var seed = _fixture.CreateDbContext())
        {
            seed.Users.AddRange(owner, member);
            seed.Projects.Add(project);
            seed.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = member.Id, CreatedAt = DateTimeOffset.UtcNow });
            await seed.SaveChangesAsync(CancellationToken.None);
        }

        await using var db = _fixture.CreateDbContext();
        db.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = member.Id, CreatedAt = DateTimeOffset.UtcNow });

        var act = async () => await db.SaveChangesAsync(CancellationToken.None);
        var thrown = await act.Should().ThrowAsync<DbUpdateException>();

        UniqueViolationMapper.IsUniqueViolation(thrown.Which).Should().BeTrue();
        UniqueViolationMapper.IsUniqueViolation(thrown.Which, "ux_team_members_project_id_user_id").Should().BeTrue();
    }

    [Fact]
    public async Task UnrelatedDbUpdateException_IsNotMappedAsUniqueViolation()
    {
        var member = NewUser("uvm-orphan@example.com");

        await using var db = _fixture.CreateDbContext();
        // No project exists for this id — violates the project_id FK, not the unique index.
        db.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), UserId = member.Id, CreatedAt = DateTimeOffset.UtcNow });

        var act = async () => await db.SaveChangesAsync(CancellationToken.None);
        var thrown = await act.Should().ThrowAsync<DbUpdateException>();

        UniqueViolationMapper.IsUniqueViolation(thrown.Which).Should().BeFalse();
    }
}
