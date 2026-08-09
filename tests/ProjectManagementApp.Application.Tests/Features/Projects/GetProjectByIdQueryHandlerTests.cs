using FluentAssertions;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Projects.GetProjectById;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using Microsoft.Extensions.Options;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

// Not-found is checked BEFORE the scope gate (data-model.md §3): an unknown id is 404 regardless of
// caller role, and a known-but-out-of-scope id is 403 only once existence is established.
[Collection(PostgresCollection.Name)]
public class GetProjectByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public GetProjectByIdQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static IOptions<ProjectsOptions> Options(bool mask = false) =>
        Microsoft.Extensions.Options.Options.Create(new ProjectsOptions { MaskOutOfScopeAs404 = mask });

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound_RegardlessOfCallerRole()
    {
        await using var db = _fixture.CreateDbContext();
        var accessPolicy = new ProjectAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(Guid.NewGuid(), "caller@example.com", "Admin", "Caller"));

        var handler = new GetProjectByIdQueryHandler(db, accessPolicy, currentUser, Options());
        var result = await handler.Handle(new GetProjectByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_KnownId_OutOfScope_ReturnsForbidden_AfterExistenceIsEstablished()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner@getbyid-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other@getbyid-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, otherPm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new ProjectAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(otherPm.Id, otherPm.Email!, "ProjectManager", otherPm.FullName));

        var handler = new GetProjectByIdQueryHandler(db, accessPolicy, currentUser, Options());
        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
    }

    [Fact]
    public async Task Handle_KnownId_OutOfScope_WithMaskingEnabled_ReturnsNotFound_InsteadOfForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner2@getbyid-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other2@getbyid-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, otherPm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new ProjectAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(otherPm.Id, otherPm.Email!, "ProjectManager", otherPm.FullName));

        var handler = new GetProjectByIdQueryHandler(db, accessPolicy, currentUser, Options(mask: true));
        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }
}
