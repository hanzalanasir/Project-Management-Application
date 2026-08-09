using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Application.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

// OQ-002-03's configurable hardening path (quickstart V5): with MaskOutOfScopeAs404 enabled, the
// distinction between "exists but forbidden" and "doesn't exist" disappears from the response.
[Collection(ApiTestCollection.Name)]
public class GetProjectByIdMaskingTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetProjectByIdMaskingTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetProjectById_OutOfScope_WithMaskingEnabled_Returns404_NotForbidden()
    {
        var setupClient = _fixture.CreateClient();
        var adminToken = await LoginAsync(setupClient, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(setupClient, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(setupClient, adminToken, "PM Two Mask", "pm2-mask@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(setupClient, "pm2-mask@example.com", "S3cure-P@ss!");
        var (id, _) = await CreateProjectAsync(setupClient, pm2Token, new CreateProjectRequest("Masked", null, "2026-08-01", null, null, null));

        var maskedClient = _fixture.CreateClient(services =>
            services.Configure<ProjectsOptions>(o => o.MaskOutOfScopeAs404 = true));

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await maskedClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
