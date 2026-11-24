using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // One exhibition. Parent of its screens, versions and sessions.
    public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> b)
        {
            b.ToTable("Projects");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.Name).HasMaxLength(Project.NameMaxLength).IsRequired();
            b.Property(p => p.Appearance).HasConversion(JsonConversions.AppearanceConverter).IsRequired();

            // The kiosk's start screen. NoAction because ModdedTemplate already cascades from Project (via ProjectId);
            // a second cascade here would create a cycle, which SQL Server rejects.
            // "The start screen cannot be deleted" is enforced in the use case (IIsMainModdedTemplateAsync).
            b.HasOne<ModdedTemplate>().WithMany()
                .HasForeignKey(p => p.MainModdedTemplateId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}