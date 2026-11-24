using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // Metadata for an uploaded file. The file itself is stored on disk (see FileStorageSettings).
    public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> b)
        {
            b.ToTable("Assets");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).ValueGeneratedNever();

            // Stored as text ("Image", "Video", "Audio") rather than a number.
            b.Property(a => a.Kind).HasConversion<string>().HasMaxLength(16).IsRequired();

            b.Property(a => a.OriginalName).HasMaxLength(255).IsRequired();

            // The file name on disk. Must be unique, otherwise two assets could overwrite each other's file.
            b.Property(a => a.StoredName).HasMaxLength(128).IsRequired();
            b.HasIndex(a => a.StoredName).IsUnique();

            b.Property(a => a.ContentType).HasMaxLength(100).IsRequired();
            b.Property(a => a.SizeBytes).IsRequired();
            b.Property(a => a.UploadedAt).IsRequired();

            // Supports filtering the media library by kind.
            b.HasIndex(a => a.Kind);
        }
    }
}