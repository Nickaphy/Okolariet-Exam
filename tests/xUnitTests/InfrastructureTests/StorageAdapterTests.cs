using Domain.Entities;                          // StaffMember
using Domain.Entities.AuthEntities;             // StaffLogin, RefreshToken, LoginLockout
using Domain.Enums;                             // StaffRole
using Domain.ValueObjects;                      // Score
using Infrastructure.Adapters.Auth;             // the auth storage adapters
using Infrastructure.Adapters.Staff;            // GetStaffByEmailAdapter
using Infrastructure.Adapters.Users;            // CreateUserAdapter
using Infrastructure.Database;                  // OkolarietDbContext
using Microsoft.EntityFrameworkCore;

namespace xUnitTests.InfrastructureTests
{
    // The auth storage adapters (logins, refresh tokens, lockouts) against EF Core's in-memory database.
    public class AuthStorageAdapterTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = new(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);

        // A fresh in-memory database per test.
        private readonly OkolarietDbContext db = new(new DbContextOptionsBuilder<OkolarietDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        [Fact]
        public async Task Saved_lockout_can_be_read_back_by_email()
        {
            await new SaveLoginLockoutAdapter(db).SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 3 }, Ct);
            db.ChangeTracker.Clear();

            var lockout = await new GetLoginLockoutAdapter(db).GetLoginLockoutAsync("anna@test.dk", Ct);

            Assert.Equal(3, lockout?.FailedAttempts);
        }

        [Fact]
        public async Task Unknown_email_has_no_lockout()
        {
            Assert.Null(await new GetLoginLockoutAdapter(db).GetLoginLockoutAsync("nobody@test.dk", Ct));
        }

        [Fact]
        public async Task Saving_a_lockout_again_replaces_the_old_one()
        {
            var save = new SaveLoginLockoutAdapter(db);
            await save.SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 1 }, Ct);
            await save.SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 4 }, Ct);
            db.ChangeTracker.Clear();

            Assert.Equal(4, (await new GetLoginLockoutAdapter(db).GetLoginLockoutAsync("anna@test.dk", Ct))?.FailedAttempts);
            Assert.Single(db.LoginLockouts);
        }

        [Fact]
        public async Task A_changed_lockout_from_Get_is_saved()
        {
            // The way LoginUseCase uses it: read, change the counter, save the same object.
            await new SaveLoginLockoutAdapter(db).SaveLoginLockoutAsync(new LoginLockout { Email = "anna@test.dk", FailedAttempts = 1 }, Ct);
            var lockout = (await new GetLoginLockoutAdapter(db).GetLoginLockoutAsync("anna@test.dk", Ct))!;
            lockout.FailedAttempts++;
            await new SaveLoginLockoutAdapter(db).SaveLoginLockoutAsync(lockout, Ct);
            db.ChangeTracker.Clear();

            Assert.Equal(2, (await new GetLoginLockoutAdapter(db).GetLoginLockoutAsync("anna@test.dk", Ct))?.FailedAttempts);
        }

        [Fact]
        public async Task Added_login_can_be_read_back_by_id()
        {
            var login = new StaffLogin { StaffId = Guid.NewGuid(), CreatedAt = Now, ExpiresAt = Now.AddHours(8) };

            await new AddLoginAdapter(db).AddLoginAsync(login, Ct);
            db.ChangeTracker.Clear();

            Assert.Equal(login.StaffId, (await new GetLoginByIdAdapter(db).GetLoginByIdAsync(login.Id, Ct))?.StaffId);
        }

        [Fact]
        public async Task Revoking_a_login_is_saved()
        {
            var login = new StaffLogin { StaffId = Guid.NewGuid(), CreatedAt = Now, ExpiresAt = Now.AddHours(8) };
            await new AddLoginAdapter(db).AddLoginAsync(login, Ct);

            var loaded = (await new GetLoginByIdAdapter(db).GetLoginByIdAsync(login.Id, Ct))!;
            loaded.RevokedAt = Now;
            await new UpdateLoginAdapter(db).UpdateLoginAsync(loaded, Ct);
            db.ChangeTracker.Clear();

            Assert.Equal(Now, (await new GetLoginByIdAdapter(db).GetLoginByIdAsync(login.Id, Ct))?.RevokedAt);
        }

        [Fact]
        public async Task RevokeAll_revokes_only_the_active_logins_of_that_staff_member()
        {
            var annaId = Guid.NewGuid();
            var annaActive = new StaffLogin { StaffId = annaId, ExpiresAt = Now.AddHours(8) };
            var annaOld = new StaffLogin { StaffId = annaId, ExpiresAt = Now.AddHours(8), RevokedAt = Now.AddDays(-1) };
            var someoneElse = new StaffLogin { StaffId = Guid.NewGuid(), ExpiresAt = Now.AddHours(8) };
            foreach (var login in new[] { annaActive, annaOld, someoneElse })
                await new AddLoginAdapter(db).AddLoginAsync(login, Ct);
            db.ChangeTracker.Clear();

            await new RevokeAllLoginsForStaffAdapter(db).RevokeAllLoginsForStaffAsync(annaId, Now, Ct);
            db.ChangeTracker.Clear();

            var get = new GetLoginByIdAdapter(db);
            Assert.Equal(Now, (await get.GetLoginByIdAsync(annaActive.Id, Ct))?.RevokedAt);
            Assert.Equal(Now.AddDays(-1), (await get.GetLoginByIdAsync(annaOld.Id, Ct))?.RevokedAt);   // keeps its original time
            Assert.Null((await get.GetLoginByIdAsync(someoneElse.Id, Ct))?.RevokedAt);
        }

        [Fact]
        public async Task Refresh_token_is_found_by_its_hash()
        {
            var token = new RefreshToken { LoginId = Guid.NewGuid(), TokenHash = "ABC123", ExpiresAt = Now.AddHours(8) };

            await new AddRefreshTokenAdapter(db).AddRefreshTokenAsync(token, Ct);
            db.ChangeTracker.Clear();

            var get = new GetRefreshTokenByHashAdapter(db);
            Assert.Equal(token.Id, (await get.GetRefreshTokenByHashAsync("ABC123", Ct))?.Id);
            Assert.Null(await get.GetRefreshTokenByHashAsync("OTHER", Ct));
        }

        [Fact]
        public async Task Marking_a_refresh_token_used_is_saved()
        {
            var token = new RefreshToken { LoginId = Guid.NewGuid(), TokenHash = "ABC123", ExpiresAt = Now.AddHours(8) };
            await new AddRefreshTokenAdapter(db).AddRefreshTokenAsync(token, Ct);

            var loaded = (await new GetRefreshTokenByHashAdapter(db).GetRefreshTokenByHashAsync("ABC123", Ct))!;
            loaded.UsedAt = Now;
            await new UpdateRefreshTokenAdapter(db).UpdateRefreshTokenAsync(loaded, Ct);
            db.ChangeTracker.Clear();

            Assert.Equal(Now, (await new GetRefreshTokenByHashAdapter(db).GetRefreshTokenByHashAsync("ABC123", Ct))?.UsedAt);
        }
    }

    // Saving a highscore (POST /users). One saved score per session and quiz.
    public class CreateUserAdapterTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly OkolarietDbContext db = new(new DbContextOptionsBuilder<OkolarietDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        [Fact]
        public async Task First_save_for_a_session_and_quiz_is_stored()
        {
            var user = User.Create("ModigRæv42", Guid.NewGuid(), Guid.NewGuid(), new Score(7, 10));

            Assert.True(await new CreateUserAdapter(db).CreateUserAsync(user, Ct));
            Assert.Single(db.Users);
        }

        [Fact]
        public async Task Second_save_for_the_same_session_and_quiz_returns_false()
        {
            var quizId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var adapter = new CreateUserAdapter(db);
            await adapter.CreateUserAsync(User.Create("ModigRæv42", quizId, sessionId, new Score(7, 10)), Ct);

            var again = await adapter.CreateUserAsync(User.Create("GladRegnorm7", quizId, sessionId, new Score(9, 10)), Ct);

            Assert.False(again);
            Assert.Single(db.Users);
        }

        [Fact]
        public async Task The_same_session_may_save_a_score_for_another_quiz()
        {
            var sessionId = Guid.NewGuid();
            var adapter = new CreateUserAdapter(db);
            await adapter.CreateUserAsync(User.Create("ModigRæv42", Guid.NewGuid(), sessionId, new Score(7, 10)), Ct);

            Assert.True(await adapter.CreateUserAsync(User.Create("ModigRæv42", Guid.NewGuid(), sessionId, new Score(5, 10)), Ct));
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
