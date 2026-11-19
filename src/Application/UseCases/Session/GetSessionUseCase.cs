using Application.ErrorMessages;
using Application.InBoundPort.ISessionUseCase;
using Application.Models;
using Application.OutBoundPort.ISessionRepository;
using FluentResults;

namespace Application.UseCases.Sessions;

public class GetSessionUseCase(
    IGetSessionSummaryAsync getSummary) : IGetSessionUseCase
{
    public async Task<Result<SessionSummary>> GetAsync(
        Guid sessionId,
        CancellationToken ct)
    {
        // Ask the read adapter for this session's progress and comparison.
        var summary = await getSummary.GetSummaryAsync(sessionId, ct);

        // The controller maps this failure to HTTP 404.
        if (summary is null)
            return Result.Fail<SessionSummary>(
                new NotFoundError("Session not found."));

        // Return the summary for the controller's HTTP 200 response.
        return Result.Ok(summary);
    }
}