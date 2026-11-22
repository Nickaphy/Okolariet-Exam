using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Application.Helpers;
using Domain.Entities;

namespace Application.OutBoundPort.IUserRepository;

public interface IListUsersAsync
{
    // Apply the optional quiz filter before counting and paging.
    // bestScore orders by Score.Percentage descending.
    // newest orders by CreatedAt descending.
    // Use Id as a final tie-breaker for stable pagination.
    // TotalCount is the filtered count before pagination.
    // Return an empty page when there are no matching entries.
    Task<PagedList<User>> ListAsync(
        Guid? moddedTemplateId,
        string sort,
        int page,
        int pageSize,
        CancellationToken ct);
}