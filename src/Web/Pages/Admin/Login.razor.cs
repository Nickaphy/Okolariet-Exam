using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Auth;

namespace Web.Pages.Admin
{
    public partial class Login
    {
        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        private readonly LoginModel model = new();
        private bool busy;
        private bool showPassword;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthState.GetAuthenticationStateAsync();
            if (state.User.Identity?.IsAuthenticated == true)
            {
                Nav.NavigateTo(SafeReturnUrl(), replace: true);
            }
        }

        private async Task LoginAsync()
        {
            busy = true;
            errorMessage = null;

            var result = await Auth.LoginAsync(model.Email!.Trim(), model.Password!);

            busy = false;
            if (result.Outcome == LoginOutcome.Success)
            {
                Nav.NavigateTo(SafeReturnUrl(), replace: true);
                return;
            }

            model.Password = string.Empty;
            errorMessage = result.Outcome switch
            {
                // Same text whether the e-mail exists or not, like the API.
                LoginOutcome.InvalidCredentials => "E-mail eller adgangskode er forkert.",
                LoginOutcome.TooManyAttempts => TooManyAttemptsText(result.RetryAfter),
                LoginOutcome.Offline => ApiErrors.Offline,
                _ => "Der skete en fejl. Prøv igen om lidt."
            };
        }

        private static string TooManyAttemptsText(TimeSpan? retryAfter)
        {
            if (retryAfter is not { } wait || wait <= TimeSpan.Zero)
                return "For mange forsøg. Vent lidt, og prøv igen.";

            var minutes = (int)Math.Ceiling(wait.TotalMinutes);
            return minutes <= 1
                ? "For mange forsøg. Prøv igen om et minut."
                : $"For mange forsøg. Prøv igen om {minutes} minutter.";
        }

        // Only allow going back to a page inside this app (blocks "//evil.example" etc.).
        private string SafeReturnUrl()
        {
            const string fallback = "admin/projects";
            if (string.IsNullOrWhiteSpace(ReturnUrl)) return fallback;

            var appRoot = new Uri(Nav.BaseUri);
            if (!Uri.TryCreate(appRoot, ReturnUrl, out var target)
                || target.GetLeftPart(UriPartial.Authority) != appRoot.GetLeftPart(UriPartial.Authority)
                || Nav.ToBaseRelativePath(target.ToString()).StartsWith("admin/login", StringComparison.OrdinalIgnoreCase))
            {
                return fallback;
            }

            return target.ToString();
        }

        private sealed class LoginModel
        {
            [Required(ErrorMessage = "Skriv din e-mail.")]
            [EmailAddress(ErrorMessage = "Det ligner ikke en e-mail.")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Skriv din adgangskode.")]
            public string? Password { get; set; }
        }
    }
}
