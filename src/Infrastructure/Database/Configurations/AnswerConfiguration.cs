using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // One answer option for a question. Reached only through its Question.
    public sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> b)
        {
            b.ToTable("Answers");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).ValueGeneratedNever();
            b.Property(a => a.Text).HasMaxLength(200).IsRequired();
            b.Property(a => a.IsCorrect).IsRequired();
            b.Property(a => a.SortOrder).IsRequired();
            b.Property(a => a.IsActive).IsRequired();   // soft delete

            // The relation to Question is configured in QuestionConfiguration.
            b.HasOne<Asset>().WithMany().HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}