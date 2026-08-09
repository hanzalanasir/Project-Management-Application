using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects", t => t.HasCheckConstraint(
            "ck_projects_end_date_after_start_date",
            "end_date IS NULL OR end_date >= start_date"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // 002 data-model.md §4: owner_id/status are the hottest scope+filter predicates; the GIN
        // trigram index is what lets ILIKE '%term%' search use an index at all (research R-3).
        builder.HasIndex(p => p.OwnerId).HasDatabaseName("ix_projects_owner_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_projects_status");
        builder.HasIndex(p => new { p.OwnerId, p.Status }).HasDatabaseName("ix_projects_owner_id_status");
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_projects_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
