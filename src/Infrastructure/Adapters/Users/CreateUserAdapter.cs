using Domain.Entities;
using Application.OutBoundPort.IUserRepository;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Users
{
    public sealed class CreateUserAdapter(OkolarietDbContext db) : ICreateUserAsync
    {
        public async Task<bool> CreateUserAsync(User user, CancellationToken ct)
        {
            if (user == null) return false; // if the user is null, return false
            db.Users.Add(user); // add the user to the database
            await db.SaveChangesAsync(ct); // save the changes to the database
            return true; // return true if the user was created successfully
        }
    }
}
