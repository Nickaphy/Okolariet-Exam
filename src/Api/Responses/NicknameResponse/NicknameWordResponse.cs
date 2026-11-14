using Domain.Entities;

namespace Api.Responses.NicknameResponse
{
    // Only id and the text, atleast for now whether or not it is active doesn't get used. 
    public record NicknameWordResponse(Guid Id, string Text)
    {
        public static NicknameWordResponse From(NicknameWord w) => new(w.Id, w.Text);
    }
}
