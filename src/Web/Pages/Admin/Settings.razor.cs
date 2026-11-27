using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;
using Web.Components;

namespace Web.Pages.Admin
{
    public partial class Settings
    {
        // Which dialog is open. Only one at a time; None = no dialog.
        private enum DialogKind { None, OwnPassword, NewStaff, StaffPassword, DeleteStaff, NewWord, DeleteWord }

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

        protected override Task OnInitializedAsync() => LoadAsync();

        // ---- loading

        // Fetches me, all staff and all words, at the same time.
        // quiet = a reload after a change: keep the page on screen and show a failure as a notice instead.
        private async Task LoadAsync(bool quiet = false)
        {
            if (!quiet) state = LoadState.Loading;

            var meTask = ApiCall.Get(() => StaffGateway.GetCurrentStaff());
            var staffTask = ApiCall.GetAll(page => StaffGateway.ListStaff(page, 100));
            var wordsTask = ApiCall.GetAll(page => NicknameGateway.ListWordsAsync(page, 100));

            var (meResult, meError) = await meTask;
            var (staffItems, staffError) = await staffTask;
            var (wordItems, wordError) = await wordsTask;

            var error = meError ?? staffError ?? wordError;   // the first error, if any
            if (error is null)
            {
                me = meResult;
                staff = staffItems;
                words = wordItems;
                state = LoadState.Loaded;
            }
            else if (quiet)
            {
                notice = Notice.Error(error);
            }
            else
            {
                loadError = error;
                state = LoadState.Failed;
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
            notice = Notice.Success(text);
            await LoadAsync(quiet: true);
        }

        // ---- my password

        private async Task<string?> ChangeOwnPasswordAsync(string currentPassword)
        {
            if (me is null) return "Din konto kunne ikke hentes. Genindlæs siden.";

            var myId = me.Id;
            var change = new PasswordChange { CurrentPassword = currentPassword, NewPassword = newPassword };
            // The server answers 422 when the current password is wrong or the new one breaks the password rules.
            return await ApiCall.Run(() => StaffGateway.SetStaffPassword(myId, change), false,
                (HttpStatusCode.UnprocessableEntity, "Den nuværende adgangskode er forkert, eller den nye opfylder ikke kravene."),
                (HttpStatusCode.BadRequest, "Den nuværende adgangskode er forkert, eller den nye er for kort."));
        }

        // Changing your own password revokes every login of yours, this one included.
        // So log out cleanly and let the user sign in with the new password.
        // (LogoutAsync never throws: without a connection it just forgets the login locally.)
        private async Task OnOwnPasswordChangedAsync(string _)
        {
            await Auth.LogoutAsync();
            Nav.NavigateTo("admin/login", replace: true);
        }

        // ---- staff

        private async Task<string?> CreateStaffAsync(string email)
        {
            if (!email.Contains('@')) return "Skriv en gyldig e-mail.";

            // The API still asks for a role. There is only one (see Application/Auth/AuthRoles on the server).
            var newStaff = new StaffCreate { Email = email, Password = newPassword, Role = StaffRole.Admin };
            return await ApiCall.Run(() => StaffGateway.CreateStaff(newStaff), false,
                (HttpStatusCode.Conflict, "Der findes allerede en medarbejder med den e-mail."),
                (HttpStatusCode.BadRequest, $"E-mailen eller adgangskoden er ikke gyldig (adgangskoden skal være mindst {MinPasswordLength} tegn)."));
        }

        private Task OnStaffCreated(string email) => Finish($"{email} er oprettet.");

        private async Task<string?> SetStaffPasswordAsync(string password)
        {
            if (password.Length < MinPasswordLength) return $"Adgangskoden skal være mindst {MinPasswordLength} tegn.";

            // An admin resetting someone else's password leaves currentPassword out.
            var change = new PasswordChange { NewPassword = password };
            return await ApiCall.Run(() => StaffGateway.SetStaffPassword(activeStaff!.Id, change));
        }

        private Task OnStaffPasswordSet(string _) => Finish($"Adgangskoden til {activeStaff?.Email} er skiftet.");

        private Task<string?> DeleteStaffAsync() =>
            ApiCall.Run(() => StaffGateway.DeleteStaff(activeStaff!.Id), ignoreNotFound: true,
                (HttpStatusCode.UnprocessableEntity, "Du kan ikke slette dig selv eller den sidste administrator."));

        private Task OnStaffDeleted() => Finish($"{activeStaff?.Email} er slettet.");

        // ---- nickname words

        private async Task<string?> CreateWordAsync(string text)
        {
            if (!WordPattern.IsMatch(text)) return "Ordet skal være 2 til 20 bogstaver, uden mellemrum eller tal.";

            return await ApiCall.Run(() => NicknameGateway.CreateNicknameWord(new NicknameWordInput { Text = text }), false,
                (HttpStatusCode.Conflict, "Ordet findes allerede."));
        }

        private Task OnWordCreated(string text) => Finish($"«{text}» er tilføjet.");

        private Task<string?> DeleteWordAsync() =>
            ApiCall.Run(() => NicknameGateway.DeleteNicknameWord(activeWord!.Id), ignoreNotFound: true,
                (HttpStatusCode.UnprocessableEntity, "Der skal være mindst 10 ord, så kiosken kan komme med forslag."));

        private Task OnWordDeleted() => Finish($"«{activeWord?.Text}» er fjernet.");
    }
}