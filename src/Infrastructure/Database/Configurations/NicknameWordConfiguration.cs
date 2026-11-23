using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    public sealed class NicknameWordConfiguration : IEntityTypeConfiguration<NicknameWord>
    {
        public void Configure(EntityTypeBuilder<NicknameWord> b)
        {
            b.ToTable("NicknameWords");
            b.HasKey(w => w.Id);
            b.Property(w => w.Id).ValueGeneratedNever();               // the entity sets its own Guid
            b.Property(w => w.Text).HasMaxLength(20).IsRequired();    // spec: 2-20 letters
            b.HasIndex(w => w.Text).IsUnique();                       // SQL Server's default collation ignores case, so "Modig" = "modig"
            b.Property(w => w.IsActive).IsRequired();
        }
    }
}
