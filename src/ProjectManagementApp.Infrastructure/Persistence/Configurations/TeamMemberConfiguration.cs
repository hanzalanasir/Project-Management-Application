using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("team_members");

        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.Project)
            .WithMany(p => p.TeamMembers)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // AddedBy is a soft reference (SET NULL) with no navigation — the membership fact
        // outlives whoever created it (data-model.md §4).
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.AddedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
