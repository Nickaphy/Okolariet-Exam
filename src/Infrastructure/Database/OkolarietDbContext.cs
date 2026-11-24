using Domain.Entities;
using Domain.Entities.AuthEntities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    // The EF Core representation of the database. Each DbSet maps to one table.
    // Every IEntityTypeConfiguration in this assembly is picked up automatically
    // in OnModelCreating, so adding a configuration file is enough to register it.
    public sealed class OkolarietDbContext(DbContextOptions<OkolarietDbContext> options) : DbContext(options)
    {
        // Staff and login
        public DbSet<StaffMember> Staff => Set<StaffMember>();
        public DbSet<StaffLogin> StaffLogins => Set<StaffLogin>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<LoginLockout> LoginLockouts => Set<LoginLockout>();

        // Exhibition content
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ModdedTemplate> ModdedTemplates => Set<ModdedTemplate>();
        public DbSet<ModdedTemplateItem> ModdedTemplateItems => Set<ModdedTemplateItem>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<PublishedVersion> PublishedVersions => Set<PublishedVersion>();

        // Guest play and results
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<QuizResult> QuizResults => Set<QuizResult>();
        public DbSet<User> Users => Set<User>();
        public DbSet<NicknameWord> NicknameWords => Set<NicknameWord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OkolarietDbContext).Assembly);
    }
}