using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // A screen in an exhibition. Aggregate root for its items (the filled-in slots).
    public sealed class ModdedTemplateConfiguration : IEntityTypeConfiguration<ModdedTemplate>
    {
        public void Configure(EntityTypeBuilder<ModdedTemplate> b)
        {
            b.ToTable("ModdedTemplates");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).ValueGeneratedNever();

            // No foreign key for TemplateType: templates are defined in code, not in the database.
            b.Property(m => m.TemplateType).HasMaxLength(64).IsRequired();
            b.Property(m => m.Title).HasMaxLength(120).IsRequired();
            b.Property(m => m.Appearance).HasConversion(JsonConversions.AppearanceConverter).IsRequired();

            b.HasOne<Project>().WithMany().HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Cascade);

            // Supports listing a project's screens in display order.
            b.HasIndex(m => new { m.ProjectId, m.SortOrder });

            // Items live in the private field "items" and are exposed read-only, so EF must use the field.
            // AutoInclude loads the items every time a ModdedTemplate is loaded, because SetItem needs to see
            // existing items to decide between "create" and "replace".
            b.HasMany(m => m.Items).WithOne().HasForeignKey(i => i.ModdedTemplateId).OnDelete(DeleteBehavior.Cascade);
            b.Navigation(m => m.Items).HasField("items").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }
}