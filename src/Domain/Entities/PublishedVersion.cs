using Domain.ValueObjects;

namespace Domain.Entities
{
    // En publiceret, uforanderlig version af et projekt. Den består af et versionsnummer,
    // et tidspunkt og et snapshot af indholdet.
    //
    // Uforanderlighed: alle properties har private set, og klassen har ingen metoder, der ændrer dem.
    // Skal kiosken have nyt indhold, oprettes en ny version med et nyt nummer. Dermed kan man altid
    // se præcist, hvad der blev vist på et givent tidspunkt.
    //
    // Aggregateroot giver klassen et Id (genereres automatisk).
    public class PublishedVersion : Aggregateroot
    {
        // Det projekt, versionen tilhører.
        public Guid ProjectId { get; private set; }

        // Løbenummer pr. projekt: 1, 2, 3 ... Hvert projekt tæller for sig.
        public int VersionNumber { get; private set; }

        // Tidspunktet for publiceringen (inkl. tidszone).
        public DateTimeOffset PublishedAt { get; private set; }

        // Selve indholdet. "null!" fortæller compileren, at feltet ikke er null efter oprettelse:
        // Create sætter den altid, og EF Core udfylder den, når versionen læses fra databasen.
        public VersionSnapshot Snapshot { get; private set; } = null!;

        private PublishedVersion() { }

        // Eneste måde at oprette en version på. Validerer input, så en ugyldig version aldrig kan eksistere.
        public static PublishedVersion Create(Guid projectId, int versionNumber, DateTimeOffset publishedAt, VersionSnapshot snapshot)
        {
            // Guid.Empty er et tomt id (kun nuller) og peger ikke på noget projekt.
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId is required.", nameof(projectId));

            // Versionsnummer 0 er reserveret til forhåndsvisning (draft), som aldrig gemmes.
            // Gemte versioner starter derfor på 1.
            if (versionNumber < 1) throw new ArgumentException("Version numbers start at 1.", nameof(versionNumber));

            ArgumentNullException.ThrowIfNull(snapshot);

            return new PublishedVersion { ProjectId = projectId, VersionNumber = versionNumber, PublishedAt = publishedAt, Snapshot = snapshot };
        }
    }
}