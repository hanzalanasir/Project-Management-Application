namespace ProjectManagementApp.Domain.Entities;

public class TeamMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid? AddedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
