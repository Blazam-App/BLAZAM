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

        private static readonly List<ResetAttempt> _resetAttempts = new();
        private readonly object _resetLock = new();


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
        private string _question1;
        private string _question2;
        private string _question3;
        private string _submittedAnswer1;
        private string _submittedAnswer2;
        private string _submittedAnswer3;
        private string _errorMessage;
        private IADUser _userToReset;
        private const string Not_Allowed_Message = "Either the user does not exist or can not reset their password";
        private GoogleAuthenticatorModal? googleAuthenticatorModal;
        private AppModal? _iFrameModal;
        private string _duoAuthUri;
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
            LoadingData = true;
            if (_appUser?.Preferences.PasswordResetSettings.PIN.IsNullOrEmpty() != false)
            {
                LoadingData = false;
                return;
            }
            if (_submittedPin.Equals(_appUser.Preferences.PasswordResetSettings.PIN?.Decrypt()))
            {


                _pinValid = true;
                _ = AttemptResetPassword();
            }
            else
            {

                SnackBarService.Error("Invalid PIN entered.");
                LoadingData = false;

            }
        }
        private async Task CheckAnswers()
        {
            LoadingData = true;
            if (_appUser?.Preferences.PasswordResetSettings.Answer1.IsNullOrEmpty() != false)
            {
                LoadingData = false;
                return;
            }
            if (CheckAnswer(_submittedAnswer1, _appUser.Preferences.PasswordResetSettings.Answer1?.Decrypt()))
            {
                if (CheckAnswer(_submittedAnswer2, _appUser.Preferences.PasswordResetSettings.Answer2?.Decrypt()))
                {
                    if (CheckAnswer(_submittedAnswer3, _appUser.Preferences.PasswordResetSettings.Answer3?.Decrypt()))
                    {
                        _qaValid = true;
                        _ = AttemptResetPassword();
                    }
                }
            }
            LoadingData = false;


        }

        private bool CheckAnswer(string submitted, string? stored)
        {
            if (stored is null || stored.Equals(String.Empty))
            {
                return false;
            }
            var su = submitted.AppTrim().ToLower();
            var st = stored.AppTrim().ToLower();
            if (!su.Equals(st, StringComparison.InvariantCultureIgnoreCase))
            {
                SnackBarService.Error("Invalid answers entered.");

                return false;
            }
            return true;
        }

        private void PasswordResetCompleted()
        {
            _appUser.Preferences.PasswordResetSettings.TokenExpiration = null;
            _appUser.Preferences.PasswordResetSettings.ResetToken = null;
            _appUser.SaveBasicUserPreferences();
            Nav.NavigateTo("/login");


        }

        private async Task StartAttemptPasswordReset()
        {
            lock (_resetLock)
            {
                if (_resetAttempts.Count(x => x.Username == LoginRequest.Username && x.AttemptTime > DateTime.UtcNow.AddMinutes(-5)) >= 1)
                {
                    SnackBarService.Error("Too many reset attempts. Please try again later.");
                    return;
                }
                foreach (var expiredAttempt in _resetAttempts.Where(x => x.AttemptTime < DateTime.UtcNow.AddMinutes(-5)))
                {
                    _resetAttempts.Remove(expiredAttempt);
                }
                _resetAttempts.Add(new ResetAttempt
                {
                    Username = LoginRequest.Username,
                    AttemptTime = DateTime.UtcNow
                });
            }
            await AttemptResetPassword();
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

                _effectiveResetPolicy = new EffectivePasswordResetPolicy(_appUser.PermissionDelegates);
                if (_effectiveResetPolicy.CanResetPassword)
                {
                    if (_effectiveResetPolicy.RequireEmail && !_tokenValid)
                    {




                        _tokenRequired = true;
                        if (_appUser.Preferences.Email.IsNullOrEmpty())
                        {
                            SnackBarService.Warning("No email configured for user");
                            return;
                        }
                        var resetToken = Guid.NewGuid().ToString();
                        var resetExpiration = DateTime.UtcNow.AddDays(1);
                        _appUser.Preferences.PasswordResetSettings.ResetToken = resetToken.ToString();

                        _appUser.Preferences.PasswordResetSettings.TokenExpiration = resetExpiration.Encrypt();
                        await _appUser.SaveBasicUserPreferences();
                        EmailService.SendPasswordResetEmail(_appUser.Preferences.Email, "/reset/" + resetToken, resetExpiration);
                        await AuditLogger.System.PasswordResetRequested(CurrentUser?.State?.IPAddress, _appUser.AuditUsername);



                        SnackBarService.Info("Verification email sent for password reset.");
                    }
                    else
                    {
                        _tokenRequired = false;
                        _tokenValid = true;

                        if ((_effectiveResetPolicy.RequirePIN && !_pinValid) ||
                            (_appUser.Preferences?.PasswordResetSettings?.PIN != null && !_pinValid))
                        {


                            if (_appUser.Preferences?.PasswordResetSettings?.PIN != null)
                            {
                                SnackBarService.Info("PIN required for password reset.");
                                _askForPin = true;
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
                        if ((_effectiveResetPolicy.RequireQA && !_qaValid) ||
                            (_appUser.Preferences?.PasswordResetSettings?.Question1 != null && !_qaValid))
                        {
                            SnackBarService.Info("Security Questions required for password reset.");
                            if (_appUser.Preferences?.PasswordResetSettings?.Question1 != null)
                            {
                                _question1 = _appUser?.Preferences.PasswordResetSettings.Question1?.Decrypt();
                                _question2 = _appUser?.Preferences.PasswordResetSettings.Question2?.Decrypt();
                                _question3 = _appUser?.Preferences.PasswordResetSettings.Question3?.Decrypt();
                                SnackBarService.Info("Security Questions required for password reset.");
                                _askForAnswers = true;
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
                            LoginRequest lr = new LoginRequest();
                            var currentUri = new Uri(Nav.Uri);
                            lr.CallbackBaseUri = currentUri.Scheme + "://" + currentUri.Authority;
                            lr.Username = _appUser.Username;
                            using var context = await DbFactory.CreateDbContextAsync();
                            var authSettings = await context.AuthenticationSettings.FirstOrDefaultAsync();
                            if(_auth.ShouldPerformDuoMFA(authSettings, lr))
                            {
                                _duoAuthUri = await _auth.PerformDuoAuthentication(lr);
                                _= _iFrameModal?.ShowAsync();
                                LoadingData = false;
                                return;
                            }
                            else if(_auth.ShouldPerformGoogleAuthenticatorMFA(_appUser.Preferences, lr, authSettings))
                            {

                            }
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
    class ResetAttempt
    {
        public string Username { get; set; }
        public DateTime AttemptTime { get; set; }
    }
}