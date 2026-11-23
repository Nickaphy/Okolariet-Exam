using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.OutBoundPort.IQuizResultRepository
{
    public interface IListQuizResultsBySessionAsync
    {
        // Outbound, returning the sessions recorded answers
        Task<IReadOnlyList<QuizResult>> ListBySessionAsync(
            Guid sessionId,
            CancellationToken ct
        );
    }
}