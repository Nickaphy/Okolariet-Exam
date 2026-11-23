using Domain.Entities;
using Domain.ValueObjects;

namespace Api.Responses.ProjectResponses
{
    // Det, klienten får tilbage: kun de felter, UI'et skal bruge, og ikke hele domæneobjektet.
    // Felterne skal have samme navne som klassen Project i Generated/Output.cs,
    // ellers kan klienten ikke læse svaret.
    public record ProjectResponse(Guid Id, string Name, Appearance Appearance, Guid? MainModdedTemplateId, int? LatestVersionNumber)
    {
        // Opretter et svar ud fra et domæneobjekt ved at kopiere værdierne over.
        public static ProjectResponse From(Project p)
            => new(p.Id, p.Name, p.Appearance, p.MainModdedTemplateId, p.LatestVersionNumber);
    }
}