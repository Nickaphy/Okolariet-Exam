using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Pages.Admin
{
    public partial class Settings
    {
        private enum LoadState { Loading, Loaded, Failed }

        private enum DialogKind { None, OwnPassword, NewStaff, StaffPassword, DeleteStaff, NewWord, DeleteWord }

        private sealed record Notice(string Kind, string Text);

        // Same rules as the API: at least 12 characters, words of 2-20 letters.
        private const int MinPasswordLength = 12;
        private static readonly Regex WordPattern = new("^[A-Za-zÆØÅæøå]{2,20}$");

        private LoadState state = LoadState.Loading;
        private string? loadError;
        private Notice? notice;

        private Staff? me;
        private List<Staff> staff = [];
        private List<NicknameWord> words = [];

        private DialogKind dialog;
        private Staff? activeStaff;
        private NicknameWord? activeWord;

        // The password fields live here (not in the dialog) because the dialogs only have one name field.
        private string newPassword = string.Empty;
        private string repeatPassword = string.Empty;

        private static string RoleName(StaffRole role) => role switch
        {
            StaffRole.Admin => "Administrator",
            _ => role.ToString()
        };

        protected override Task OnInitializedAsync() => LoadAsync();

        // ---- loading

        // quiet = reload after a change without flashing the "Henter…" text.
        private async Task LoadAsync(bool quiet = false)
        {
            if (!quiet) state = LoadState.Loading;

            try
            {
                var meTask = StaffApi.GetCurrentStaff();
                var staffTask = LoadStaffAsync();
                var wordsTask = LoadWordsAsync();

                var meResponse = await meTask;
                var (staffItems, staffError) = await staffTask;
                var (wordItems, wordError) = await wordsTask;

                var error = ApiErrors.ErrorOrNull(meResponse) ?? staffError ?? wordError;
                if (error is not null)
                {
                    SetLoadFailed(error, quiet);
                    return;
                }

                me = meResponse.Content;
                staff = staffItems;
                words = wordItems;
                state = LoadState.Loaded;
            }
            catch (HttpRequestException)
            {
                SetLoadFailed(ApiErrors.Offline, quiet);
            }
        }

        private void SetLoadFailed(string error, bool quiet)
        {
            if (quiet)
            {
                notice = new Notice("error", error);
                return;
            }

            loadError = error;
            state = LoadState.Failed;
        }

        private async Task<(List<Staff> Items, string? Error)> LoadStaffAsync()
        {
            var all = new List<Staff>();
            for (var page = 1; ; page++)
            {
                var response = await StaffApi.ListStaff(page, 100);
                if (!response.IsSuccessStatusCode || response.Content is null) return (all, ApiErrors.Describe(response));

                all.AddRange(response.Content.Items);
                if (response.Content.Items.Count == 0 || all.Count >= response.Content.TotalCount) return (all, null);
            }
        }

        private async Task<(List<NicknameWord> Items, string? Error)> LoadWordsAsync()
        {
            var all = new List<NicknameWord>();
            for (var page = 1; ; page++)
            {
                var response = await NicknamesApi.ListNicknameWords(page, 100);
                if (!response.IsSuccessStatusCode || response.Content is null) return (all, ApiErrors.Describe(response));

                all.AddRange(response.Content.Items);
                if (response.Content.Items.Count == 0 || all.Count >= response.Content.TotalCount) return (all, null);
            }
        }

        // ---- opening and closing dialogs

        private void ResetPasswordFields()
        {
            newPassword = string.Empty;
            repeatPassword = string.Empty;
        }

        private void AskOwnPassword()
        {
            ResetPasswordFields();
            dialog = DialogKind.OwnPassword;
        }

        private void AskNewStaff()
        {
            ResetPasswordFields();
            dialog = DialogKind.NewStaff;
        }

        private void AskStaffPassword(Staff member)
        {
            activeStaff = member;
            dialog = DialogKind.StaffPassword;
        }

        private void AskDeleteStaff(Staff member)
        {
            activeStaff = member;
            dialog = DialogKind.DeleteStaff;
        }

        private void AskNewWord() => dialog = DialogKind.NewWord;

        private void AskDeleteWord(NicknameWord word)
        {
            activeWord = word;
            dialog = DialogKind.DeleteWord;
        }

        private void CloseDialog()
        {
            dialog = DialogKind.None;
            activeStaff = null;
            activeWord = null;
            ResetPasswordFields();
        }

        // Closes the dialog, tells the user what happened and reloads the lists.
        private async Task Finish(string text)
        {
            CloseDialog();
            notice = new Notice("success", text);
            await LoadAsync(quiet: true);
        }

        // ---- my password

        private async Task<string?> ChangeOwnPasswordAsync(string currentPassword)
        {
            if (me is null) return "Din konto kunne ikke hentes. Genindlæs siden.";

            var response = await StaffApi.SetStaffPassword(me.Id, new PasswordChange
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            });

            return ApiErrors.ErrorOrNull(response, false,
                (HttpStatusCode.BadRequest, "Den nuværende adgangskode er forkert, eller den nye er for kort."));
        }

        // Changing your own password revokes every login of yours, this one included.
        // So log out cleanly and let the user sign in with the new password.
        private async Task OnOwnPasswordChangedAsync(string _)
        {
            try
            {
                await Auth.LogoutAsync();
            }
            catch (HttpRequestException)
            {
                // The server already ended the login. Nothing more to clean up here.
            }

            Nav.NavigateTo("admin/login", replace: true);
        }

        // ---- staff

        private async Task<string?> CreateStaffAsync(string email)
        {
            if (!email.Contains('@')) return "Skriv en gyldig e-mail.";

            var response = await StaffApi.CreateStaff(new StaffCreate
            {
                Email = email,
                Password = newPassword,
                Role = StaffRole.Admin
            });

            return ApiErrors.ErrorOrNull(response, false,
                (HttpStatusCode.Conflict, "Der findes allerede en medarbejder med den e-mail."),
                (HttpStatusCode.BadRequest, $"E-mailen eller adgangskoden er ikke gyldig (adgangskoden skal være mindst {MinPasswordLength} tegn)."));
        }

        private Task OnStaffCreated(string email) => Finish($"{email} er oprettet.");

        private async Task<string?> SetStaffPasswordAsync(string password)
        {
            if (password.Length < MinPasswordLength) return $"Adgangskoden skal være mindst {MinPasswordLength} tegn.";

            // An admin resetting someone else's password leaves currentPassword out.
            var response = await StaffApi.SetStaffPassword(activeStaff!.Id, new PasswordChange { NewPassword = password });
            return ApiErrors.ErrorOrNull(response);
        }

        private Task OnStaffPasswordSet(string _) => Finish($"Adgangskoden til {activeStaff?.Email} er skiftet.");

        private async Task<string?> DeleteStaffAsync() =>
            ApiErrors.ErrorOrNull(await StaffApi.DeleteStaff(activeStaff!.Id), true,
                (HttpStatusCode.UnprocessableEntity, "Du kan ikke slette dig selv eller den sidste administrator."));

        private Task OnStaffDeleted() => Finish($"{activeStaff?.Email} er slettet.");

        // ---- nickname words

        private async Task<string?> CreateWordAsync(string text)
        {
            if (!WordPattern.IsMatch(text)) return "Ordet skal være 2 til 20 bogstaver, uden mellemrum eller tal.";

            var response = await NicknamesApi.CreateNicknameWord(new NicknameWordInput { Text = text });
            return ApiErrors.ErrorOrNull(response, false,
                (HttpStatusCode.Conflict, "Ordet findes allerede."));
        }

        private Task OnWordCreated(string text) => Finish($"«{text}» er tilføjet.");

        private async Task<string?> DeleteWordAsync() =>
            ApiErrors.ErrorOrNull(await NicknamesApi.DeleteNicknameWord(activeWord!.Id), true,
                (HttpStatusCode.UnprocessableEntity, "Der skal være mindst 10 ord, så kiosken kan komme med forslag."));

        private Task OnWordDeleted() => Finish($"«{activeWord?.Text}» er fjernet.");
    }
}