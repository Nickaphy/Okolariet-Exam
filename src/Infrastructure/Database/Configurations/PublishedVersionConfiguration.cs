using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // An immutable, published snapshot of a project. This is what the kiosk plays.
    public sealed class PublishedVersionConfiguration : IEntityTypeConfiguration<PublishedVersion>
    {
        public void Configure(EntityTypeBuilder<PublishedVersion> b)
        {
            b.ToTable("PublishedVersions");
            b.HasKey(v => v.Id);
            b.Property(v => v.Id).ValueGeneratedNever();
            b.Property(v => v.VersionNumber).IsRequired();
            b.Property(v => v.PublishedAt).IsRequired();

            // The whole snapshot is stored as JSON (nvarchar(max)) and read as one unit.
            b.Property(v => v.Snapshot).HasConversion(JsonConversions.SnapshotConverter).IsRequired();

            b.HasOne<Project>().WithMany().HasForeignKey(v => v.ProjectId).OnDelete(DeleteBehavior.Cascade);

            // Two concurrent publishes cannot get the same version number: the second insert fails,
            // and the adapter can translate that into a conflict.
            b.HasIndex(v => new { v.ProjectId, v.VersionNumber }).IsUnique();
        }
    }
}