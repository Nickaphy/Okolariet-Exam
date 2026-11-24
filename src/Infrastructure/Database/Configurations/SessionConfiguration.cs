using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // One anonymous guest visit at the kiosk.
    public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> b)
        {
            b.ToTable("Sessions");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();

            // StartedAt is get-only in the domain. Mapping it explicitly lets EF use its backing field.
            b.Property(s => s.StartedAt).IsRequired();

            // Deleting a project deletes the sessions collected for it (and through them, results and users).
            b.HasOne<Project>().WithMany().HasForeignKey(s => s.ProjectId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(s => s.ProjectId);
        }
    }
}