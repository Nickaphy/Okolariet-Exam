using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // Failed login attempts per e-mail, used to lock an account for 15 minutes after 5 failures.
    public sealed class LoginLockoutConfiguration : IEntityTypeConfiguration<LoginLockout>
    {
        public void Configure(EntityTypeBuilder<LoginLockout> b)
        {
            b.ToTable("LoginLockouts");

            // There is no Id: the normalised (lower-case) e-mail is the primary key.
            b.HasKey(l => l.Email);
            b.Property(l => l.Email).HasMaxLength(254).ValueGeneratedNever();
            b.Property(l => l.FailedAttempts).IsRequired();
        }
    }
}