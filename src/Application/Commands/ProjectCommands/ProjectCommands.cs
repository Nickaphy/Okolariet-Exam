using Domain.ValueObjects;

namespace Application.Commands.ProjectCommands
{
    // Et command samler de oplysninger, et use case skal bruge. Controlleren oversætter sin Request
    // til et command, så Application-laget ikke afhænger af Api-laget (afhængigheder må kun pege nedad).
    // Commands er records: rene dataobjekter uden adfærd.

    // Oplysninger til oprettelse af et projekt.
    public record CreateProjectCommand(string Name, Guid? MainModdedTemplateId, Appearance? Appearance);

    // Oplysninger til opdatering: id'et på det projekt, der skal ændres, plus de nye værdier.
    public record UpdateProjectCommand(Guid Id, string Name, Guid? MainModdedTemplateId, Appearance? Appearance);

    // Sletning kræver kun id'et.
    public record DeleteProjectCommand(Guid Id);
}