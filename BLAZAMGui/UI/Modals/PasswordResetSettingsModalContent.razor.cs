
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Gui.UI.Modals
{
    public partial class PasswordResetSettingsModalContent : AppModalContent
    {
        private string pin = "";
        private string question1 = "";
        private string answer1 = "";
        private string question2 = "";
        private string answer2 = "";
        private string question3 = "";
        private string answer3 = "";

        private bool HasSettings => !string.IsNullOrEmpty(pin) ||
                                    !string.IsNullOrEmpty(question1) ||
                                    !string.IsNullOrEmpty(question2) ||
                                    !string.IsNullOrEmpty(question3);

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            Modal.SetYesText(AppLocalization[Lang.Save]);
            Modal.SetOnYes(SaveSettings);
            Modal.YesEnabled = () => { return !LoadingData && IsValid; };

            await LoadExistingSettings();
        }

        private async Task LoadExistingSettings()
        {
            try
            {
                LoadingData = true;
                await Modal.RefreshView();

                var dbUser = await Context.UserSettings
                    .Include(u => u.PasswordResetSettings)
                    .FirstOrDefaultAsync(u => u.Id == CurrentUser.State.Id);

                if (dbUser?.PasswordResetSettings != null)
                {
                    // Decrypt for editing
                    pin = dbUser.PasswordResetSettings.PIN?.Decrypt() ?? "";
                    question1 = dbUser.PasswordResetSettings.Question1?.Decrypt() ?? "";
                    answer1 = dbUser.PasswordResetSettings.Answer1?.Decrypt() ?? "";
                    question2 = dbUser.PasswordResetSettings.Question2?.Decrypt() ?? "";
                    answer2 = dbUser.PasswordResetSettings.Answer2?.Decrypt() ?? "";
                    question3 = dbUser.PasswordResetSettings.Question3?.Decrypt() ?? "";
                    answer3 = dbUser.PasswordResetSettings.Answer3?.Decrypt() ?? "";
                }
            }
            catch (Exception ex)
            {
                SnackBarService.Error(AppLocalization[Lang.Error] + " " + ex.Message);
            }
            finally
            {
                LoadingData = false;
                await Modal.RefreshView();
            }
        }

        private async Task SaveSettings()
        {
            LoadingData = true;
            await Modal.RefreshView();

            try
            {
                var dbUser = await Context.UserSettings
                    .Include(u => u.PasswordResetSettings)
                    .FirstOrDefaultAsync(u => u.Id == CurrentUser.State.Id);

                if (dbUser == null)
                {
                    SnackBarService.Error(AppLocalization[Lang.Error]);
                    return;
                }

                var pr = dbUser.PasswordResetSettings;
                if (pr == null)
                {
                    pr = new Database.Models.User.UserPasswordResetSettings()
                    {
                        UserId = dbUser.Id
                    };
                    dbUser.PasswordResetSettings = pr;
                }

                // Encrypt values before saving
                pr.PIN = string.IsNullOrEmpty(pin) ? null : await pin.EncryptAsync();
                pr.Question1 = string.IsNullOrEmpty(question1) ? null : await question1.EncryptAsync();
                pr.Answer1 = string.IsNullOrEmpty(answer1) ? null : await answer1.EncryptAsync();
                pr.Question2 = string.IsNullOrEmpty(question2) ? null : await question2.EncryptAsync();
                pr.Answer2 = string.IsNullOrEmpty(answer2) ? null : await answer2.EncryptAsync();
                pr.Question3 = string.IsNullOrEmpty(question3) ? null : await question3.EncryptAsync();
                pr.Answer3 = string.IsNullOrEmpty(answer3) ? null : await answer3.EncryptAsync();

                await Context.SaveChangesAsync();

                // Keep in-memory state in sync
                CurrentUser.State.Preferences.PasswordResetSettings = dbUser.PasswordResetSettings;

                SnackBarService.Success(AppLocalization[Lang.Settings_saved]);
                await Modal.CloseAsync();
            }
            catch (Exception ex)
            {
                SnackBarService.Error(AppLocalization[Lang.Failed_to_save_settings] + " " + ex.Message);
            }
            finally
            {
                LoadingData = false;
                await Modal.RefreshView();
            }
        }

        private async Task ClearSettings()
        {
            if (!await MessageService.Confirm(AppHelpLocalization["Confirm_Clear_Password_Reset_Settings"], AppLocalization[Lang.Confirm_deletion]))
            {
                return;
            }

            LoadingData = true;
            await Modal.RefreshView();
            try
            {
                var dbUser = await Context.UserSettings
                    .Include(u => u.PasswordResetSettings)
                    .FirstOrDefaultAsync(u => u.Id == CurrentUser.State.Id);

                if (dbUser?.PasswordResetSettings != null)
                {
                    dbUser.PasswordResetSettings.PIN = null;
                    dbUser.PasswordResetSettings.Question1 = null;
                    dbUser.PasswordResetSettings.Answer1 = null;
                    dbUser.PasswordResetSettings.Question2 = null;
                    dbUser.PasswordResetSettings.Answer2 = null;
                    dbUser.PasswordResetSettings.Question3 = null;
                    dbUser.PasswordResetSettings.Answer3 = null;
                    dbUser.PasswordResetSettings.ResetToken = null;
                    dbUser.PasswordResetSettings.TokenExpiration = null;

                    await Context.SaveChangesAsync();

                    // update UI state
                    pin = question1 = answer1 = question2 = answer2 = question3 = answer3 = "";
                    CurrentUser.State.Preferences.PasswordResetSettings = null;

                    SnackBarService.Success(AppLocalization[Lang.Changes_have_been_saved]);
                    await Modal.CloseAsync();
                }
                else
                {
                    SnackBarService.Info(AppLocalization[Lang.No_matching_results]);
                }
            }
            catch (Exception ex)
            {
                SnackBarService.Error(AppLocalization[Lang.Error] + " " + ex.Message);
            }
            finally
            {
                LoadingData = false;
                await Modal.RefreshView();
            }
        }
    }
}