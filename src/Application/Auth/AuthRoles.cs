using Domain.Enums;

namespace Application.Auth
{
    // The role names used for authorization. There is one role today: every staff member is an Admin.
    // This was changed since a normal User can never access the editor without logging in, and it helps prevent every controller from using Role = Admin.
    //
    // Where the role is used, so it is enforced in exactly ONE place:
    // - JwtTokenGenerator writes it into the access token as the "role" claim (StaffRole.Admin → "Admin");
    // - the Api's fallback policy (ApiDependencyInjection) requires it on every endpoint that is not
    //   [AllowAnonymous] (kiosk) or [Authorize] (any logged-in staff member, e.g. log out).
    // Controllers do NOT repeat [Authorize(Roles = ...)]. A new role (e.g. a read-only "Editor") would be
    // a new StaffRole value, a constant here, and a named policy on the endpoints it may use.
    public static class AuthRoles
    {
        public const string Admin = nameof(StaffRole.Admin);
    }
}
