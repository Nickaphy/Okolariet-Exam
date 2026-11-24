using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // One guest's answer to one question, with the server's verdict.
    public sealed class QuizResultConfiguration : IEntityTypeConfiguration<QuizResult>
    {
        public void Configure(EntityTypeBuilder<QuizResult> b)
        {
            b.ToTable("QuizResults");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).ValueGeneratedNever();

            // All four properties are get-only in the domain, so they are mapped explicitly.
            b.Property(r => r.SessionId).IsRequired();
            b.Property(r => r.QuestionId).IsRequired();
            b.Property(r => r.AnswerId).IsRequired();
            b.Property(r => r.IsCorrect).IsRequired();

            // Results belong to a session and are deleted with it. "Reset results for a project"
            // is therefore a single delete of the project's sessions.
            b.HasOne<Session>().WithMany().HasForeignKey(r => r.SessionId).OnDelete(DeleteBehavior.Cascade);

            // NoAction: questions and answers already reach this table through the project's cascade path.
            b.HasOne<Question>().WithMany().HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne<Answer>().WithMany().HasForeignKey(r => r.AnswerId).OnDelete(DeleteBehavior.NoAction);

            // A question can be answered only once per session, enforced even under concurrent requests
            // (which is why the port is called ITryAddQuizResultAsync).
            b.HasIndex(r => new { r.SessionId, r.QuestionId }).IsUnique();

            // Supports per-question statistics.
            b.HasIndex(r => r.QuestionId);
        }
    }
}