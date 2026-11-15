using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects
{
    public class PasswordPolicy
    {
        public int MinLength { get; init; } = 8;
        public int MaxLength { get; init; } = 128;
        public bool RequireUppercase { get; init; } = true;
        public bool RequireLowercase { get; init; } = true;
        public bool RequireDigit { get; init; } = true;
        public bool RequireSpecialChar { get; init; } = true;
        public bool DisallowWhitespace { get; init; } = true;
        public bool DisallowPersonalInfo { get; init; } = true;
        public int MinPersonalInfoFragmentLength { get; init; } = 3;
        public HashSet<string> DisallowedPasswords { get; init; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "password1",
            "12345678",
            "qwerty123",
            "admin123"
        };
    }
}
