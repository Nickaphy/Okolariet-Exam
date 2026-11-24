using Domain.Entities.AuthEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokens");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).ValueGeneratedNever();

            // Only the hash is stored, never the raw token. Every lookup goes through the hash, hence the unique index.
            b.Property(t => t.TokenHash).HasMaxLength(256).IsRequired();
            b.HasIndex(t => t.TokenHash).IsUnique();

            b.Property(t => t.CreatedAt).IsRequired();
            b.Property(t => t.ExpiresAt).IsRequired();

            // A token belongs to a login and is deleted with it.
            b.HasOne<StaffLogin>().WithMany().HasForeignKey(t => t.LoginId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}