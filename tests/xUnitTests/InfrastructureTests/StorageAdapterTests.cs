using Domain.Entities;                          // StaffMember
using Domain.Entities.AuthEntities;             // StaffLogin, RefreshToken
using Domain.Enums;                             // StaffRole
using Domain.ValueObjects;                      // LoginLockout
using Infrastructure.Adapters.Staff;            // GetStaffByEmailAdapter
using Infrastructure.Database;                  // OkolarietDbContext
using Infrastructure.Testing;                   // InMemoryAuthStore
using Microsoft.EntityFrameworkCore;

namespace xUnitTests.InfrastructureTests
{
    public class InMemoryAuthStoreTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = new(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);

        private readonly InMemoryAuthStore store = new();

        [Fact]
        public async Task Saved_lockout_can_be_read_back_by_email()
        {
            await store.SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 3 }, Ct);

            var lockout = await store.GetLoginLockoutAsync("anna@test.dk", Ct);

            Assert.Equal(3, lockout?.FailedAttempts);
        }

        [Fact]
        public async Task Unknown_email_has_no_lockout()
        {
            Assert.Null(await store.GetLoginLockoutAsync("nobody@test.dk", Ct));
        }

        [Fact]
        public async Task Saving_a_lockout_again_replaces_the_old_one()
        {
            await store.SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 1 }, Ct);
            await store.SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 4 }, Ct);

            Assert.Equal(4, (await store.GetLoginLockoutAsync("anna@test.dk", Ct))?.FailedAttempts);
        }

        [Fact]
        public async Task Added_login_can_be_read_back_by_id()
        {
            var login = new StaffLogin { StaffId = Guid.NewGuid(), CreatedAt = Now, ExpiresAt = Now.AddHours(8) };

            await store.AddLoginAsync(login, Ct);

            Assert.Same(login, await store.GetLoginByIdAsync(login.Id, Ct));
        }

        [Fact]
        public async Task RevokeAll_revokes_only_the_active_logins_of_that_staff_member()
        {
            var annaId = Guid.NewGuid();
            var annaActive = new StaffLogin { StaffId = annaId, ExpiresAt = Now.AddHours(8) };
            var annaOld = new StaffLogin { StaffId = annaId, ExpiresAt = Now.AddHours(8), RevokedAt = Now.AddDays(-1) };
            var someoneElse = new StaffLogin { StaffId = Guid.NewGuid(), ExpiresAt = Now.AddHours(8) };
            foreach (var login in new[] { annaActive, annaOld, someoneElse })
                await store.AddLoginAsync(login, Ct);

            await store.RevokeAllLoginsForStaffAsync(annaId, Now, Ct);

            Assert.Equal(Now, annaActive.RevokedAt);
            Assert.Equal(Now.AddDays(-1), annaOld.RevokedAt);   // keeps its original time
            Assert.Null(someoneElse.RevokedAt);
        }

        [Fact]
        public async Task Refresh_token_is_found_by_its_hash()
        {
            var token = new RefreshToken { LoginId = Guid.NewGuid(), TokenHash = "ABC123", ExpiresAt = Now.AddHours(8) };

            await store.AddRefreshTokenAsync(token, Ct);

            Assert.Same(token, await store.GetRefreshTokenByHashAsync("ABC123", Ct));
            Assert.Null(await store.GetRefreshTokenByHashAsync("OTHER", Ct));
        }
    }

    public class GetStaffByEmailAdapterTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        // A fresh in-memory database per test (unique name), so tests don't see each other's data.
        private static OkolarietDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<OkolarietDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new OkolarietDbContext(options);
        }

        private static async Task<OkolarietDbContext> DbWithAnna()
        {
            var db = NewDb();
            db.Staff.Add(new StaffMember { Id = Guid.NewGuid(), Email = "anna@test.dk", PasswordHash = "x", Role = StaffRole.Admin });
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            return db;
        }

        [Fact]
        public async Task Finds_staff_member_by_email()
        {
            await using var db = await DbWithAnna();

            var staff = await new GetStaffByEmailAdapter(db).GetByEmailAsync("anna@test.dk", Ct);

            Assert.Equal("anna@test.dk", staff?.Email);
        }

        [Fact]
        public async Task Unknown_email_returns_null()
        {
            await using var db = await DbWithAnna();

            Assert.Null(await new GetStaffByEmailAdapter(db).GetByEmailAsync("nobody@test.dk", Ct));
        }

        [Fact]
        public async Task Adapter_expects_an_already_normalised_email()
        {
            // The adapter compares exactly; LoginUseCase normalises with EmailNormalizer first.
            await using var db = await DbWithAnna();

            Assert.Null(await new GetStaffByEmailAdapter(db).GetByEmailAsync("ANNA@test.dk", Ct));
        }

        [Fact]
        public async Task Returned_staff_member_is_not_tracked()
        {
            // AsNoTracking: reading for login must not leave the entity attached to the context.
            await using var db = await DbWithAnna();

            await new GetStaffByEmailAdapter(db).GetByEmailAsync("anna@test.dk", Ct);

            Assert.Empty(db.ChangeTracker.Entries());
        }
    }

    public class StaffMemberConfigurationTests
    {
        // SQL Server provider, but no connection is ever opened: we only inspect the model.
        private static Microsoft.EntityFrameworkCore.Metadata.IEntityType StaffEntity()
        {
            var options = new DbContextOptionsBuilder<OkolarietDbContext>()
                .UseSqlServer("Server=unused;Database=unused;TrustServerCertificate=True")
                .Options;
            using var db = new OkolarietDbContext(options);
            return db.Model.FindEntityType(typeof(StaffMember))!;
        }

        [Fact]
        public void Staff_are_stored_in_the_Staff_table()
        {
            Assert.Equal("Staff", StaffEntity().GetTableName());
        }

        [Fact]
        public void Email_is_unique_and_max_254_characters()
        {
            var entity = StaffEntity();
            var email = entity.FindProperty(nameof(StaffMember.Email))!;

            Assert.Equal(254, email.GetMaxLength());
            Assert.False(email.IsNullable);
            Assert.Contains(entity.GetIndexes(), i => i.IsUnique && i.Properties.Single() == email);
        }

        [Fact]
        public void Role_is_stored_as_text()
        {
            var role = StaffEntity().FindProperty(nameof(StaffMember.Role))!;

            Assert.Equal(typeof(string), role.GetProviderClrType());   // "Admin", not 0
        }

        [Fact]
        public void Id_is_set_by_the_code_not_the_database()
        {
            var id = StaffEntity().FindProperty(nameof(StaffMember.Id))!;

            Assert.Equal(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never, id.ValueGenerated);
        }
    }
}
