namespace Application.Helpers
{
    // Trims an Email address and converts it to lower-case. This is useful when comparing address'
    public static class EmailNormalizer
    {
        public static string Normalize(string email) => email.Trim().ToLowerInvariant();
    }
}
