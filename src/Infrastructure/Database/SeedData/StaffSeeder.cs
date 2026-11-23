using Application.Commands.StaffCommands;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IStaffRepository;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database
{
    // Creates the first admin when the Staff table is empty, using SeedAdmin__Email and SeedAdmin__Password from .env.
    // It goes through CreateStaffUseCase, so the seeded admin gets the same e-mail normalisation,
    // password policy and hashing as any other staff member.
    public sealed class StaffSeeder(IAnyAsync anyStaff, ICreateStaffUseCase createStaff, ILogger<StaffSeeder> logger)
    {
        public async Task SeedAsync(string? email, string? password, CancellationToken ct = default)
        {
            if (await anyStaff.AnyAsync(ct))
                return;   // staff already exist: never overwrite or add a second seed admin

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    "No staff members exist and SeedAdmin__Email / SeedAdmin__Password are missing in .env, so nobody could log in.");

            var result = await createStaff.CreateAsync(new CreateStaffCommand(email, password, StaffRole.Admin), ct);
            if (result.IsFailed)   // e.g. the password doesn't meet the PasswordPolicy
                throw new InvalidOperationException(
                    "Could not create the first admin from .env: " + string.Join(" ", result.Errors.Select(e => e.Message)));

            logger.LogInformation("Created the first admin from SeedAdmin settings.");
        }
    }
}