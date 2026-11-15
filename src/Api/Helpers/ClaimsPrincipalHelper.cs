using System.Security.Claims;

namespace Api.Helpers
{
    // This is a helper class to get the StaffId from the ClaimsPrincipal, which is used in the controllers to get the currently logged in staff member's ID.
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetStaffId(this ClaimsPrincipal user) =>
            Guid.TryParse(user.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

        // Used to find the LoginId of the active user. Needed for token generation
        public static Guid GetLoginId(this ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue("sid")!);
    }
}
