using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.OutBoundPort.IModdedTemplateRepository
{
    // Resonsible for that the questions quiz belongs to the project the guest is visiting
    public interface ITemplateBelongsToProjectAsync
    {
        Task<bool> BelongsToProjectAsync(
            Guid moddedTemplateId,
            Guid projectId,
            CancellationToken ct
        );
    }
}