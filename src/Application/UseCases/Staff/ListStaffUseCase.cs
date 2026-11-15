using System;
using System.Collections.Generic;
using System.Text;
using Application.Helpers;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Staff
{
    public class ListStaffUseCase : IListStaffUseCase
    {
        private readonly IListAsync list;

        public ListStaffUseCase(IListAsync list)
        {
            this.list = list;
        }

        public async Task<Result<PagedList<StaffMember>>> ListAsync(int page, int pageSize, CancellationToken ct)
        => Result.Ok(await list.ListAsync(page, pageSize, ct));
    }
}
