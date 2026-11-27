using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // A guest's saved quiz score under a nickname
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Users");
            b.HasKey(u => u.Id);
            b.Property(u => u.Id).ValueGeneratedNever();

            // Two words plus a number, so 64 characters is ample.
            b.Property(u => u.NickName).HasMaxLength(64).IsRequired();
            b.Property(u => u.CreatedAt).IsRequired();

            // Score is a value object stored as two columns on Users (Correct, TotalQuestions).
            // Percentage is calculated from them and is not stored.
            b.OwnsOne(u => u.Score, s =>
            {
                s.Property(x => x.Correct).HasColumnName("Correct").IsRequired();
                s.Property(x => x.TotalQuestions).HasColumnName("TotalQuestions").IsRequired();
                s.Ignore(x => x.Percentage);
            });
            b.Navigation(u => u.Score).IsRequired();

            b.HasOne<Session>().WithMany().HasForeignKey(u => u.SessionId).OnDelete(DeleteBehavior.Cascade);

            // NoAction: Users is already reached through Session, so a second cascade would create multiple paths.
            b.HasOne<ModdedTemplate>().WithMany().HasForeignKey(u => u.ModdedTemplateId).OnDelete(DeleteBehavior.NoAction);

            // Supports the leaderboard per quiz.
            b.HasIndex(u => new { u.ModdedTemplateId, u.CreatedAt });

            // One saved score per session and quiz (the API's rule, otherwise 409). The database enforces it,
            // so it also holds when two requests arrive at the same moment (see CreateUserAdapter).
            b.HasIndex(u => new { u.SessionId, u.ModdedTemplateId }).IsUnique();
        }
    }
}