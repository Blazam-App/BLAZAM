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
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Pages
{
    public partial class Reset : ValidatedForm
    {
        private AppModal? _resetModel;
        private IApplicationUserState? _appUser;
        private EffectivePasswordResetPolicy? _effectiveResetPolicy = null;
        private bool _tokenValid;
        private bool _qaValid;
        private bool _pinValid;
        private bool _attemptReset;
        private bool _askForPin;
        private bool _askForAnswers;
        private bool _tokenRequired;
        private string _submittedPin;
        private string _errorMessage;
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
        private async Task CheckPin()
        {
            if (_appUser?.Preferences.PasswordResetSettings.PIN.IsNullOrEmpty()!=false)
            {
                return;
            }
            if (_submittedPin.Equals(_appUser.Preferences.PasswordResetSettings.PIN?.Decrypt()))
            {
                

                _pinValid = true;
                _= AttemptResetPassword();
            }
            else
            {
                SnackBarService.Error("Invalid PIN entered.");
            }
        }
        private async Task AttemptResetPassword()
        {
            _attemptReset = true;
            LoadingData = true;
            try
            {
                var user = Directory.Users.FindUserByUsername(LoginRequest.Username, exactMatch: true);
                if (user is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
                _appUser = await user.GetApplicationUser(UserStateService, DbFactory, Directory, AppAuthenticationStateProvider);
                if (_appUser is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }

                var eprp = new EffectivePasswordResetPolicy(_appUser.PermissionDelegates);
                if (eprp.CanResetPassword)
                {
                    if (eprp.RequireEmail && !_tokenValid)
                    {
                        _tokenRequired = true;
                        if (_appUser.Preferences.Email.IsNullOrEmpty())
                        {
                            SnackBarService.Warning("No email configured for user");
                            return;
                        }
                        var resetToken = Guid.NewGuid().ToString();
                        _appUser.Preferences.PasswordResetSettings.ResetToken = resetToken.ToString();
                        _appUser.Preferences.PasswordResetSettings.TokenExpiration = DateTime.UtcNow.AddDays(1).Encrypt();
                        await _appUser.SaveBasicUserPreferences();
                        EmailService.SendPasswordResetEmail(_appUser.Preferences.Email, "/reset/" + resetToken);


                        SnackBarService.Info("Verification email sent for password reset.");
                    }
                    else
                    {
                        _tokenRequired = false;
                        _tokenValid = true;
                        _effectiveResetPolicy = eprp;
                        if (eprp.RequirePIN && !_pinValid)
                        {
                            if (_appUser.Preferences?.PasswordResetSettings?.PIN != null)
                            {
                                SnackBarService.Info("PIN required for password reset.");
                                _askForPin = true;
                                _askForAnswers = false;
                            }
                            else
                            {
                                SnackBarService.Error("PIN required but not configured.");
                                _errorMessage = "Your account is not configured for account recovery, please contact your system administrator.";
                            }
                        }
                        else
                        {
                            _pinValid = true;
                        }
                        if (eprp.RequireQA && !_qaValid)
                        {
                            SnackBarService.Info("Security Questions required for password reset.");
                            if (_appUser.Preferences?.PasswordResetSettings?.Question1 != null)
                            {
                                SnackBarService.Info("Security Questions required for password reset.");
                                _askForAnswers = true;
                                _askForPin = false;
                            }
                            else
                            {
                                SnackBarService.Error("Security Questions required but not configured.");
                                _errorMessage = "Your account is not configured for account recovery, please contact your system administrator.";
                            }
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