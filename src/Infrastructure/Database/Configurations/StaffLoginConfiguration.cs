using Domain.Entities;
using Domain.Entities.AuthEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    // One login session for a staff member. Refresh tokens hang off a login.
    public sealed class StaffLoginConfiguration : IEntityTypeConfiguration<StaffLogin>
    {
        public void Configure(EntityTypeBuilder<StaffLogin> b)
        {
            b.ToTable("StaffLogins");
            b.HasKey(l => l.Id);
            b.Property(l => l.Id).ValueGeneratedNever();   // the entity creates its own Guid
            b.Property(l => l.CreatedAt).IsRequired();
            b.Property(l => l.ExpiresAt).IsRequired();

            // Deleting a staff member also deletes their logins.
            b.HasOne<StaffMember>().WithMany().HasForeignKey(l => l.StaffId).OnDelete(DeleteBehavior.Cascade);

            // Supports "revoke all logins for this staff member".
            b.HasIndex(l => l.StaffId);
        }
    }
}