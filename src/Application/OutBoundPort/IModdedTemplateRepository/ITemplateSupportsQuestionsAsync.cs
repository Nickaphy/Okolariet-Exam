namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface ITemplateSupportsQuestionsAsync
    {
        Task<bool> SupportsQuestionsAsync(Guid moddedTemplateId, CancellationToken ct);
    }
}