using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Statistics;

namespace Web.Adapters.Statistics
{
    public class StatisticsGateway(IStatisticsApi api) : IStatisticsGateway
    {
        public async Task<ProjectStatistics> GetProjectStatistics(Guid projectId, CancellationToken ct = default)
            => (await api.GetProjectStatistics(projectId, ct)).Unwrap();

        public async Task<QuestionStatistics> GetQuestionStatistics(Guid questionId, CancellationToken ct = default)
            => (await api.GetQuestionStatistics(questionId, ct)).Unwrap();

        public async Task DeleteProjectResults(Guid projectId, CancellationToken ct = default)
            => (await api.DeleteProjectResults(projectId, ct)).EnsureSuccess();

        public async Task DeleteQuestionResults(Guid questionId, CancellationToken ct = default)
            => (await api.DeleteQuestionResults(questionId, ct)).EnsureSuccess();
    }
}
