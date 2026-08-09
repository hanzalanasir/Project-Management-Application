using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

[Collection(ApiTestCollection.Name)]
public class CreateProjectAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateProjectAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task CreateProject_AsTeamMember_Returns403_AndNothingIsWritten()
    {
        var client = _fixture.CreateClient();
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectRequest("Should Not Exist", null, "2026-08-01", null, null, null)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
