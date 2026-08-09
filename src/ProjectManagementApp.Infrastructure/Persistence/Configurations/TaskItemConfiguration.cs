using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

        builder.Property(t => t.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 003 data-model.md §4: (project_id) and (assignee_id) already exist as EF's default
        // FK-index convention — declared here anyway for readability/discoverability alongside
        // the four indexes that don't. (status) serves the ?status= filter; the composite pairs
        // serve "one project's board" and "my tasks by status" (005/006); the GIN trigram index is
        // what lets ILIKE '%term%' use an index at all (same mechanism as 002's project-name search).
        builder.HasIndex(t => t.ProjectId).HasDatabaseName("ix_tasks_project_id");
        builder.HasIndex(t => t.AssigneeId).HasDatabaseName("ix_tasks_assignee_id");
        builder.HasIndex(t => t.Status).HasDatabaseName("ix_tasks_status");
        builder.HasIndex(t => new { t.ProjectId, t.Status }).HasDatabaseName("ix_tasks_project_id_status");
        builder.HasIndex(t => new { t.AssigneeId, t.Status }).HasDatabaseName("ix_tasks_assignee_id_status");
        builder.HasIndex(t => t.Title)
            .HasDatabaseName("ix_tasks_title_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
