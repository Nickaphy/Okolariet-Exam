using Domain.Entities;                       // NicknameWord
using Infrastructure.Adapters.Nickname;      // the nickname adapters
using Infrastructure.Database;               // OkolarietDbContext
using Microsoft.EntityFrameworkCore;

namespace xUnitTests.InfrastructureTests
{
    public class NicknameWordAdapterTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        // A fresh in-memory database per test.
        private readonly OkolarietDbContext db = new(new DbContextOptionsBuilder<OkolarietDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        [Fact]
        public async Task Added_word_can_be_read_back_by_id()
        {
            var word = NicknameWord.Create("Modig");

            await new AddNicknameWordAdapter(db).AddAsync(word, Ct);
            db.ChangeTracker.Clear();
            var found = await new GetNicknameWordAdapter(db).GetAsync(word.Id, Ct);

            Assert.Equal("Modig", found?.Text);
        }

        [Fact]
        public async Task Get_unknown_id_returns_null()
        {
            Assert.Null(await new GetNicknameWordAdapter(db).GetAsync(Guid.NewGuid(), Ct));
        }

        [Theory]
        [InlineData("Modig")]
        [InlineData("modig")]
        [InlineData("MODIG")]
        public async Task Exists_ignores_case(string text)
        {
            await new AddNicknameWordAdapter(db).AddAsync(NicknameWord.Create("Modig"), Ct);

            Assert.True(await new NicknameWordExistsAdapter(db).ExistsAsync(text, Ct));
        }

        [Fact]
        public async Task Exists_is_false_for_a_new_word()
        {
            await new AddNicknameWordAdapter(db).AddAsync(NicknameWord.Create("Modig"), Ct);

            Assert.False(await new NicknameWordExistsAdapter(db).ExistsAsync("Glad", Ct));
        }

        [Fact]
        public async Task Removed_word_is_gone()
        {
            var word = NicknameWord.Create("Modig");
            await new AddNicknameWordAdapter(db).AddAsync(word, Ct);

            var tracked = await new GetNicknameWordAdapter(db).GetAsync(word.Id, Ct);
            await new RemoveNicknameWordAdapter(db).RemoveAsync(tracked!, Ct);

            Assert.Null(await new GetNicknameWordAdapter(db).GetAsync(word.Id, Ct));
        }

        [Fact]
        public async Task CountActive_skips_inactive_words()
        {
            var add = new AddNicknameWordAdapter(db);
            await add.AddAsync(NicknameWord.Create("Modig"), Ct);
            await add.AddAsync(NicknameWord.Create("Glad"), Ct);
            await add.AddAsync(NicknameWord.Create("Gammel", isActive: false), Ct);

            Assert.Equal(2, await new CountActiveNicknameWordsAdapter(db).CountActiveAsync(Ct));
        }
    }

    public class NicknameWordConfigurationTests
    {
        // SQL Server provider without a connection: only the model is inspected.
        private static Microsoft.EntityFrameworkCore.Metadata.IEntityType Entity()
        {
            var options = new DbContextOptionsBuilder<OkolarietDbContext>()
                .UseSqlServer("Server=unused;Database=unused;TrustServerCertificate=True")
                .Options;
            using var db = new OkolarietDbContext(options);
            return db.Model.FindEntityType(typeof(NicknameWord))!;
        }

        [Fact]
        public void Words_are_stored_in_the_NicknameWords_table()
        {
            Assert.Equal("NicknameWords", Entity().GetTableName());
        }

        [Fact]
        public void Text_is_unique_required_and_max_20_characters()
        {
            var entity = Entity();
            var text = entity.FindProperty(nameof(NicknameWord.Text))!;

            Assert.Equal(20, text.GetMaxLength());
            Assert.False(text.IsNullable);
            Assert.Contains(entity.GetIndexes(), i => i.IsUnique && i.Properties.Single() == text);
        }
    }
}