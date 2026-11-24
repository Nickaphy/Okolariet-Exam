using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // The content of one slot on one screen. Reached only through its ModdedTemplate.
    public sealed class ModdedTemplateItemConfiguration : IEntityTypeConfiguration<ModdedTemplateItem>
    {
        public void Configure(EntityTypeBuilder<ModdedTemplateItem> b)
        {
            b.ToTable("ModdedTemplateItems");
            b.HasKey(i => i.Id);
            b.Property(i => i.Id).ValueGeneratedNever();
            b.Property(i => i.SlotKey).HasMaxLength(64).IsRequired();
            b.Property(i => i.Text).HasMaxLength(500);

            // A slot holds at most one item per screen. This is what makes SetItem an upsert.
            b.HasIndex(i => new { i.ModdedTemplateId, i.SlotKey }).IsUnique();

            // The relation to the owning ModdedTemplate is configured in ModdedTemplateConfiguration.
            // NoAction on the two relations below avoids multiple cascade paths. Deleting an asset or a screen
            // that is still referenced is prevented in the use cases (IAssetInUseAsync, IIsLinkedFromButtonAsync).
            b.HasOne<Asset>().WithMany().HasForeignKey(i => i.AssetId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne<ModdedTemplate>().WithMany().HasForeignKey(i => i.TargetModdedTemplateId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}