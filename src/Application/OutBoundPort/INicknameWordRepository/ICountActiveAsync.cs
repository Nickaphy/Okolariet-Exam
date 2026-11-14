using System;
using System.Collections.Generic;
using System.Text;

namespace Application.OutBoundPort.INicknameWordRepository
{
    public interface ICountActiveAsync
    {
        Task<int> CountActiveAsync(CancellationToken ct);
    }
}
