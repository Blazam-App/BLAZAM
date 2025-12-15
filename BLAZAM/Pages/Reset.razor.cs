// Import necessary namespaces for various functionalities
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Global.Enums;
using BLAZAM.Gui.UI;
using BLAZAM.Gui.UI.Modals;
using BLAZAM.Gui.UI.Settings;
using BLAZAM.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Pages
{
    public partial class Reset : ValidatedForm
    {
        private AppModal? _resetModel;
        private EffectivePasswordResetPolicy? _effectiveResetPolicy = null;
        private bool _tokenValid;
        private bool _qaValid;
        private bool _pinValid;
        private bool _tokenRequired = true;
        private IADUser _userToReset;
        private const string Not_Allowed_Message = "Either the user does not exist or can not reset their password";
        private GoogleAuthenticatorModal? googleAuthenticatorModal;

        [Parameter]
        public string Token { get; set; }
        private LoginRequest LoginRequest = new();


        /// <summary>
        /// Asynchronously initializes the component and sets up the necessary state and event handlers.
        /// </summary>
        /// <remarks>This method sets the redirect URL and callback base URI for the login request based
        /// on the current navigation URI.  Additionally, it subscribes to the <see cref="ConnMonitor.OnAppReadyChanged"/>
        /// event if the application is not in the  <see cref="ServiceConnectionState.Up"/> state.</remarks>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (!Token.IsNullOrEmpty())
            {
                var usersettings = await Context.UserSettings.Include(x => x.PasswordResetSettings).FirstOrDefaultAsync(x => x.PasswordResetSettings.ResetToken == Token);
                if (usersettings?.PasswordResetSettings.TokenExpiration?.Decrypt<DateTime>() > DateTime.UtcNow)
                {
                    _tokenValid = true;
                    _userToReset = Directory.Users.FindUserBySID(usersettings.UserGUID);
                    if (_userToReset != null)
                    {
                        LoginRequest.Username = _userToReset.SAMAccountName;
                        await AttemptResetPassword();
                        SnackBarService.Success("Token Valid!");
                    }
                }
                else
                {
                    SnackBarService.Error("Either the token has expired or the token is invalid");
                }
            }

            LoadingData = false;
        }

        private async Task AttemptResetPassword()
        {
            LoadingData = true;
            try
            {
                var user = Directory.Users.FindUserByUsername(LoginRequest.Username, exactMatch: true);
                if (user is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
                var appUser = await user.GetApplicationUser(UserStateService, DbFactory, Directory, AppAuthenticationStateProvider);
                if (appUser is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }

                var eprp = new EffectivePasswordResetPolicy(appUser.PermissionDelegates);
                if (eprp.CanResetPassword)
                {
                    if (eprp.RequireEmail && !_tokenValid)
                    {
                        if (appUser.Preferences.Email.IsNullOrEmpty())
                        {
                            SnackBarService.Warning("No email configured for user");
                            return;
                        }
                        var resetToken = Guid.NewGuid().ToString();
                        appUser.Preferences.PasswordResetSettings.ResetToken = resetToken.ToString();
                        appUser.Preferences.PasswordResetSettings.TokenExpiration = DateTime.UtcNow.AddDays(1).Encrypt();
                        await appUser.SaveBasicUserPreferences();
                        EmailService.SendPasswordResetEmail(appUser.Preferences.Email, "https://" + DatabaseCache.ApplicationSettings.AppFQDN + "/reset/" + resetToken);


                        SnackBarService.Info("Verification email sent for password reset.");
                    }
                    else
                    {
                        _tokenRequired = false;
                        _effectiveResetPolicy = eprp;
                        if (eprp.RequirePIN)
                        {

                            SnackBarService.Info("PIN required for password reset.");
                        }
                        else
                        {
                            _pinValid = true;
                        }
                        if (eprp.RequireQA)
                        {
                            SnackBarService.Info("Security Questions required for password reset.");
                        }
                        else
                        {
                            _qaValid = true;
                        }
                        if (_pinValid && _qaValid)
                        {
                            _resetModel?.ShowAsync();
                        }
                    }
                }
                else
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
            }
            finally
            {
                LoadingData = false;
            }
        }

    }
}