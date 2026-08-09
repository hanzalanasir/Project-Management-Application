using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

[Collection(ApiTestCollection.Name)]
public class DeleteProjectTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public DeleteProjectTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task DeleteProject_Returns204_CascadesDependents_RetainsAuditRow_SecondDeleteReturns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("To Delete", null, "2026-08-01", null, "Planning", null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AssignTeamMemberAsync(_fixture.Services, id, tmId);

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var owner = await db.Projects.Where(p => p.Id == id).Select(p => p.OwnerId).SingleAsync();
            db.Tasks.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "A task",
                ProjectId = id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(CancellationToken.None);
        }

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var deleteResponse = await client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Tasks.CountAsync(t => t.ProjectId == id)).Should().Be(0);
            (await db.TeamMembers.CountAsync(tm => tm.ProjectId == id)).Should().Be(0);

            var auditRow = await db.ActivityLogs.SingleOrDefaultAsync(a => a.EntityId == id.ToString() && a.Action == "ProjectDeleted");
            auditRow.Should().NotBeNull();
            auditRow!.ChangeSummary.Should().Contain("To Delete");
        }

        using var secondDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{id}");
        secondDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var secondDeleteResponse = await client.SendAsync(secondDeleteRequest);
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
