using Web.Api.Contracts;

namespace Web.Ports.Statistics
{
    // How guests answered: totals per exhibition and per question, and starting those numbers over (admin).
    public interface IStatisticsGateway
    {
        Task<ProjectStatistics> GetProjectStatistics(Guid projectId, CancellationToken ct = default);
        Task<QuestionStatistics> GetQuestionStatistics(Guid questionId, CancellationToken ct = default);
        Task DeleteProjectResults(Guid projectId, CancellationToken ct = default);
        Task DeleteQuestionResults(Guid questionId, CancellationToken ct = default);
    }
}
