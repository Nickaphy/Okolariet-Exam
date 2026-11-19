using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.OutBoundPort.IQuizResultRepository
{
    public interface ITryAddQuizResultAsync
    {
        // Returns false if this session already answered this question
        Task<bool> TryAddAsync(
            QuizResult quizResult,
            CancellationToken ct
        );
    }
}