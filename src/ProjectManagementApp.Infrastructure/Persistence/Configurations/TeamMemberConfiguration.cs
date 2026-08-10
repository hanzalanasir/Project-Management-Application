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

        // The UNIQUE index is this feature's actual correctness guarantee (data-model.md §4,
        // research R-2/R-3) — one membership per (project, user), and what makes the concurrent
        // duplicate-add race resolve to one 201 and one 409 rather than two rows or a lost update.
        // The other two are performance-only, serving the roster read and the cascade-cleanup path.
        builder.HasIndex(m => new { m.ProjectId, m.UserId })
            .IsUnique()
            .HasDatabaseName("ux_team_members_project_id_user_id");
        builder.HasIndex(m => m.ProjectId).HasDatabaseName("ix_team_members_project_id");
        builder.HasIndex(m => m.UserId).HasDatabaseName("ix_team_members_user_id");
    }
}
