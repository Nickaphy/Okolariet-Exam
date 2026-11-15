using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{

    public sealed class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
    {
        public void Configure(EntityTypeBuilder<StaffMember> b)
        {
            b.ToTable("Staff");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();          // we set Guid ourselves
            b.Property(s => s.Email).HasMaxLength(254).IsRequired();
            b.HasIndex(s => s.Email).IsUnique();                  // backs the 409 duplicate rule
            b.Property(s => s.PasswordHash).IsRequired();
            b.Property(s => s.Role).HasConversion<string>().HasMaxLength(32);   // stored as "Admin"
            b.Property(s => s.CreatedAt).IsRequired();
        }
    }
}
