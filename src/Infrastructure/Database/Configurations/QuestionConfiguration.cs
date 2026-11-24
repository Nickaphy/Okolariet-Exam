using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // A quiz question. Aggregate root for its answers.
    public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> b)
        {
            b.ToTable("Questions");
            b.HasKey(q => q.Id);
            b.Property(q => q.Id).ValueGeneratedNever();
            b.Property(q => q.Text).HasMaxLength(500).IsRequired();
            b.Property(q => q.SortOrder).IsRequired();

            // Soft delete (IsActive = false), so historical statistics survive.
            b.Property(q => q.IsActive).IsRequired();

            b.HasOne<ModdedTemplate>().WithMany().HasForeignKey(q => q.ModdedTemplateId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Asset>().WithMany().HasForeignKey(q => q.AssetId).OnDelete(DeleteBehavior.NoAction);
            b.HasIndex(q => new { q.ModdedTemplateId, q.SortOrder });

            // Answers live in the private field "answers". AutoInclude, because rules such as
            // HasExactlyOneCorrectAnswer need all answers to be loaded.
            b.HasMany(q => q.Answers).WithOne().HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);
            b.Navigation(q => q.Answers).HasField("answers").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }
}