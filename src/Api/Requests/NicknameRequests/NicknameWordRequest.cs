using System.ComponentModel.DataAnnotations;

namespace Api.Requests.NicknameRequests
{
    // Requires the length to be a max of 20 and minimum of 2. The RegularExpression allows Danish characters. A-Z (Allows uppercase letters), a-z (Allows lowercase letters), ÆØÅæøå (Allows Danish characters).
    public record NicknameWordRequest(
        [property: Required, StringLength(20, MinimumLength = 2),
     RegularExpression("^[A-Za-zÆØÅæøå]+$")] string Text);
}
