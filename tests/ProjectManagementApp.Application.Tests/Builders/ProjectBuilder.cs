using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Builders;

// Constitution IX.4 — test data via builders, not inline object literals scattered across files.
// From builders, never the production seeder (ADR-0007 §4).
public class ProjectBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Project";
    private string? _description;
    private DateOnly _startDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly? _endDate;
    private ProjectStatus _status = ProjectStatus.Planning;
    private Guid _ownerId = Guid.NewGuid();
    private ApplicationUser? _owner;
    private uint _version;

    public ProjectBuilder WithId(Guid id) { _id = id; return this; }
    public ProjectBuilder WithName(string name) { _name = name; return this; }
    public ProjectBuilder WithDescription(string? description) { _description = description; return this; }
    public ProjectBuilder WithStartDate(DateOnly startDate) { _startDate = startDate; return this; }
    public ProjectBuilder WithEndDate(DateOnly? endDate) { _endDate = endDate; return this; }
    public ProjectBuilder WithStatus(ProjectStatus status) { _status = status; return this; }
    public ProjectBuilder WithOwnerId(Guid ownerId) { _ownerId = ownerId; return this; }
    public ProjectBuilder WithOwner(ApplicationUser owner) { _owner = owner; _ownerId = owner.Id; return this; }
    public ProjectBuilder WithVersion(uint version) { _version = version; return this; }

    public Project Build()
    {
        var now = DateTimeOffset.UtcNow;
        var project = new Project
        {
            Id = _id,
            Name = _name,
            Description = _description,
            StartDate = _startDate,
            EndDate = _endDate,
            Status = _status,
            OwnerId = _ownerId,
            CreatedAt = now,
            UpdatedAt = now,
            Version = _version,
        };

        if (_owner is not null)
        {
            project.Owner = _owner;
        }

        return project;
    }
}
