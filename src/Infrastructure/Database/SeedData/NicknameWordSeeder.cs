using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database
{
    // Inserts a starter list of nickname words when the table is empty.
    // The kiosk needs at least 10 active words to suggest nicknames, and DeleteWordUseCase refuses to go below 10,
    // so a fresh database must start with enough words.
    // The words satisfy the same rule as NicknameWordRequest: letters only, 2-20 characters.
    public sealed class NicknameWordSeeder(OkolarietDbContext db, ILogger<NicknameWordSeeder> logger)
    {
        private static readonly string[] Words =
        [
            "Modig", "Glad", "Sej", "Klog", "Hurtig", "Stærk", "Venlig", "Snu", "Rolig", "Frisk",
            "Odder", "Delfin", "Pingvin", "Skildpadde", "Hval", "Sæl", "Ræv", "Ugle", "Pindsvin", "Egern"
        ];

        public async Task SeedAsync(CancellationToken ct = default)
        {
            // If any word exists, staff may have edited the list, so it is never added to or overwritten.
            if (await db.NicknameWords.AnyAsync(ct))
                return;

            db.NicknameWords.AddRange(Words.Select(w => NicknameWord.Create(w)));
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} nickname words.", Words.Length);
        }
    }
}