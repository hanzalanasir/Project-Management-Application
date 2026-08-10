using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Builders;

// Constitution IX.4 — test data via builders, not inline object literals scattered across files.
// From builders, never the production seeder (ADR-0007 §4).
public class TeamMemberBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _projectId = Guid.NewGuid();
    private Project? _project;
    private Guid _userId = Guid.NewGuid();
    private ApplicationUser? _user;
    private Guid? _addedBy;

    public TeamMemberBuilder WithId(Guid id) { _id = id; return this; }
    public TeamMemberBuilder WithProjectId(Guid projectId) { _projectId = projectId; return this; }
    public TeamMemberBuilder WithProject(Project project) { _project = project; _projectId = project.Id; return this; }
    public TeamMemberBuilder WithUserId(Guid userId) { _userId = userId; return this; }
    public TeamMemberBuilder WithUser(ApplicationUser user) { _user = user; _userId = user.Id; return this; }
    public TeamMemberBuilder WithAddedBy(Guid? addedBy) { _addedBy = addedBy; return this; }

    public TeamMember Build()
    {
        var member = new TeamMember
        {
            Id = _id,
            ProjectId = _projectId,
            UserId = _userId,
            AddedBy = _addedBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (_project is not null)
        {
            member.Project = _project;
        }

        if (_user is not null)
        {
            member.User = _user;
        }

        return member;
    }
}
