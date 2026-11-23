using System.Reflection.Emit;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{

    public sealed class OkolarietDbContext(DbContextOptions<OkolarietDbContext> options) : DbContext(options)
    {
        public DbSet<StaffMember> Staff => Set<StaffMember>();
        public DbSet<NicknameWord> NicknameWords => Set<NicknameWord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OkolarietDbContext).Assembly);
    }
}
