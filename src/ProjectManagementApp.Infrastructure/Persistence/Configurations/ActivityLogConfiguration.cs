using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(64).IsRequired();
        builder.Property(a => a.ChangeSummary).HasMaxLength(1000).IsRequired();

        // Deliberately no FK on ActorId — audit must outlive the actor (data-model.md §4).
        // Same reasoning for ProjectId: it must outlive the project (ProjectDeleted rows survive
        // deletion), so it is a plain nullable column, never a foreign key.

        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.ActorId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.ProjectId);
    }
}
