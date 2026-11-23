using Domain.ValueObjects;

namespace Domain.Entities
{
    // Et projekt er én udstilling
    // fælles udseende og startskærm, og alle udstillingens skærme (ModdedTemplates) hører til det.
    //
    // Aggregateroot giver klassen et Id, som genereres automatisk ved oprettelse.
    public class Project : Aggregateroot
    {
        // Maksimal længde på navnet. Grænsen er defineret i kontrakten (openapi.yaml).
        public const int NameMaxLength = 120;

        // "private set" betyder, at værdien kan læses udefra, men kun ændres af klassen selv.
        // Dermed kan et projekt ikke få et ugyldigt navn ved at sætte feltet direkte.
        // Startværdien "" sikrer, at navnet aldrig er null.
        public string Name { get; private set; } = "";

        // Udstillingens fælles udseende. Starter som et tomt udseende (alle felter ikke valgt).
        public Appearance Appearance { get; private set; } = Appearance.Empty;

        // Id'et på den skærm, kiosken viser først (startskærmen). Null betyder, at ingen er valgt endnu,
        // hvilket er tilfældet for et nyt projekt, som endnu ikke har nogen skærme.
        public Guid? MainModdedTemplateId { get; private set; }

        // Nummeret på den seneste publicerede version. Null betyder, at projektet aldrig er publiceret.
        public int? LatestVersionNumber { get; private set; }

        // Kun til EF Core, som skal kunne oprette et tomt objekt og fylde det ud fra databasen.
        // Privat, så andre ikke kan oprette et projekt uden validering.
        private Project() { }

        // Valideringsreglerne for navnet. Static, så reglerne kan bruges uden at have et projekt.
        // ArgumentException er standardfejlen for "ugyldig inputværdi".
        public static void Validate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name cannot be empty.", nameof(name));

            // Trim() fjerner mellemrum i begge ender, så de ikke tæller med i længden.
            if (name.Trim().Length > NameMaxLength)
                throw new ArgumentException($"Project name cannot be longer than {NameMaxLength} characters.", nameof(name));
        }

        // Eneste måde at oprette et nyt projekt på, så validering altid gennemføres.
        // Udseendet er valgfrit: "appearance ?? Appearance.Empty" betyder "brug det angivne udseende,
        // eller et tomt udseende hvis intet er angivet". Appearance på et projekt er derfor aldrig null.
        public static Project Create(string name, Appearance? appearance = null)
        {
            Validate(name);
            return new Project { Name = name.Trim(), Appearance = appearance ?? Appearance.Empty };
        }

        // Ændrer navn, startskærm og udseende. Navnet valideres igen.
        //
        // Reglen "startskærmen skal tilhøre dette projekt" kontrolleres ikke her, fordi Project ikke
        // kender sine skærme. Den kontrolleres i UpdateProjectUseCase, som kan slå op i databasen.
        public void Update(string name, Guid? mainModdedTemplateId, Appearance? appearance)
        {
            Validate(name);
            Name = name.Trim();
            MainModdedTemplateId = mainModdedTemplateId;
            Appearance = appearance ?? Appearance.Empty;
        }

        // Kaldes af PublishVersionUseCase, når projektet er publiceret, så projektet kender
        // nummeret på sin seneste version.
        public void RegisterPublishedVersion(int versionNumber) => LatestVersionNumber = versionNumber;
    }
}