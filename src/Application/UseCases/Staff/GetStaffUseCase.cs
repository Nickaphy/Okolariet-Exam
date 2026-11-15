using Application.ErrorMessages;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Staff
{
    // Use case for retrieving a staff member by ID
    public class GetStaffUseCase : IGetStaffUseCase
    {
        private readonly IGetByIdAsync getById;

        public GetStaffUseCase(IGetByIdAsync getById)
        {
            this.getById = getById;
        }

        public async Task<Result<StaffMember>> GetAsync(Guid id, CancellationToken ct)
        {
            var member = await getById.GetByIdAsync(id, ct);
            return member is null
                ? Result.Fail<StaffMember>(new NotFoundError("Staff member not found."))
                : Result.Ok(member);
        }
    }
}
