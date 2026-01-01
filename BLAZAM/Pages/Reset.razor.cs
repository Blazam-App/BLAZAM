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
using BLAZAM.Services.Audit;
using BLAZAM.Services.Duo;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Pages
{
    /// <summary>
    /// Provides functionality for handling the user password reset process, including validation of reset tokens, PINs,
    /// security questions, and multi-factor authentication (MFA) as required by policy.
    /// </summary>
    /// <remarks>The Reset component manages the workflow for securely resetting a user's password. It
    /// supports multiple verification steps such as email token validation, PIN entry, security questions, and MFA (Duo
    /// or Google Authenticator), depending on the configured reset policy. The component is intended to be used in
    /// scenarios where users need to recover or reset their credentials in a secure and auditable manner. This class is
    /// not thread-safe and should be used per request or per user session. Implements IDisposable to release resources
    /// associated with audit logging.</remarks>
    public partial class Reset : ValidatedForm, IDisposable
    {

        private static List<ResetAttempt> _resetAttempts = new();
        private readonly object _resetLock = new();


        private AuthenticationSettings authSettings;

        private AppModal? _resetModel;
        private IApplicationUserState? _appUser;
        private WebUserAuditLogger? _resetAuditLogger;
        private IApplicationUserState? appUser
        {
            get => _appUser; set
            {
                if (_appUser == value)
                {
                    return;
                }
                _appUser = value;
                if (value != null)
                {
                    _resetAuditLogger = new WebUserAuditLogger(DbFactory, value, JS);

                }
            }
        }
        private EffectivePasswordResetPolicy? _effectiveResetPolicy = null;
        private bool _tokenValid;
        private bool _qaValid;
        private bool _pinValid;
        private bool _mfaValid;
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
        private string _duoAuthUri;
        private LoginRequest LoginRequest = new();

        /// <summary>
        /// Gets or sets the state parameter returned from the MFA provider during the password reset process.
        /// </summary>
        [SupplyParameterFromQuery(Name = "state")]
        public string? State { get; set; }
        /// <summary>
        /// Gets or sets the authorization code returned from the MFA provider during the password reset process.   
        /// </summary>
        [SupplyParameterFromQuery(Name = "code")]
        public string? Code { get; set; }
        /// <summary>
        /// Gets or sets the password reset token used to validate the user's password reset request.
        /// </summary>
        [Parameter]
        public string Token { get; set; }




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

            var currentUri = new Uri(Nav.Uri);
            LoginRequest.CallbackBaseUri = currentUri.Scheme + "://" + currentUri.Authority;

            using var context = await DbFactory.CreateDbContextAsync();
            authSettings = await context.AuthenticationSettings.FirstOrDefaultAsync();

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
            if (!State.IsNullOrEmpty() && !Code.IsNullOrEmpty())
            {
                //Duo callback
                DuoClientProvider duoClientProvider = new(DbFactory);
                var resetReq = PasswordResetService.GetPasswordResetRequestFromMFAToken(State);
                _userToReset = resetReq.User;
                if (await duoClientProvider.VerifyCallback(Nav.BaseUri + "reset", _userToReset.SAMAccountName, Code))
                {
                    Token = resetReq.Token;
                    LoginRequest.Username = _userToReset.SAMAccountName;
                    _mfaValid = true;
                    _tokenValid = true;
                    _pinValid = true;
                    _qaValid = true;
                    AttemptResetPassword();
                }

            }
            LoadingData = false;
        }
        /// <summary>
        /// Asynchronously checks the submitted PIN against the stored PIN for the user.
        /// </summary>
        /// <returns></returns>
        private async Task CheckPinAsync()
        {
            LoadingData = true;
            if (appUser?.Preferences.PasswordResetSettings.PIN.IsNullOrEmpty() != false)
            {
                LoadingData = false;
                return;
            }
            if (_submittedPin.Equals(appUser.Preferences.PasswordResetSettings.PIN?.Decrypt()))
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
        /// <summary>
        /// Asynchronously checks the submitted answers to security questions against the stored answers for the user.
        /// </summary>
        /// <returns></returns>
        private async Task CheckAnswersAsync()
        {
            LoadingData = true;
            if (appUser?.Preferences.PasswordResetSettings.Answer1.IsNullOrEmpty() != false)
            {
                LoadingData = false;
                return;
            }
            if (CheckAnswer(_submittedAnswer1, appUser.Preferences.PasswordResetSettings.Answer1?.Decrypt())
                && CheckAnswer(_submittedAnswer2, appUser.Preferences.PasswordResetSettings.Answer2?.Decrypt())
                && CheckAnswer(_submittedAnswer3, appUser.Preferences.PasswordResetSettings.Answer3?.Decrypt()))
            {
                _qaValid = true;
                _ = AttemptResetPassword();
                return;
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
            appUser.Preferences.PasswordResetSettings.TokenExpiration = null;
            appUser.Preferences.PasswordResetSettings.ResetToken = null;
            appUser.SaveBasicUserPreferences();
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
                _resetAttempts = _resetAttempts.Where(x => x.AttemptTime > DateTime.UtcNow.AddMinutes(-5)).ToList();

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
            await Task.Delay(10);
            await StateHasChangedAsync();
            try
            {
                var user = Directory.Users.FindUserByUsername(LoginRequest.Username, exactMatch: true);
                if (user is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
                appUser = await user.GetApplicationUser(UserStateService, DbFactory, Directory, AppAuthenticationStateProvider);
                if (appUser is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
                _effectiveResetPolicy = new EffectivePasswordResetPolicy(appUser.PermissionDelegates);
                if (_effectiveResetPolicy.CanResetPassword)
                {
                    if (_effectiveResetPolicy.RequireEmail && !_tokenValid)
                    {
                        _tokenRequired = true;
                        if (appUser.Preferences.Email.IsNullOrEmpty())
                        {
                            SnackBarService.Warning("No email configured for user");
                            return;
                        }
                        var resetToken = Guid.NewGuid().ToString();
                        var resetExpiration = DateTime.UtcNow.AddDays(1);
                        appUser.Preferences.PasswordResetSettings.ResetToken = resetToken.ToString();

                        appUser.Preferences.PasswordResetSettings.TokenExpiration = await resetExpiration.EncryptAsync();
                        await appUser.SaveBasicUserPreferences();
                        EmailService.SendPasswordResetEmail(appUser.Preferences.Email, "/reset/" + resetToken, resetExpiration);
                        await AuditLogger.System.PasswordResetRequested(CurrentUser?.State?.IPAddress, appUser.AuditUsername);



                        SnackBarService.Info("Verification email sent for password reset.");
                        return;
                    }

                    _tokenRequired = false;
                    _tokenValid = true;

                    if ((_effectiveResetPolicy.RequirePIN && !_pinValid) ||
                        (appUser.Preferences?.PasswordResetSettings?.PIN != null && !_pinValid))
                    {


                        if (appUser.Preferences?.PasswordResetSettings?.PIN != null)
                        {
                            SnackBarService.Info("PIN required for password reset.");
                            _askForPin = true;
                            return;
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
                        (appUser.Preferences?.PasswordResetSettings?.Question1 != null && !_qaValid))
                    {
                        SnackBarService.Info("Security Questions required for password reset.");
                        if (appUser.Preferences?.PasswordResetSettings?.Question1 != null)
                        {
                            _question1 = appUser?.Preferences.PasswordResetSettings.Question1?.Decrypt();
                            _question2 = appUser?.Preferences.PasswordResetSettings.Question2?.Decrypt();
                            _question3 = appUser?.Preferences.PasswordResetSettings.Question3?.Decrypt();
                            SnackBarService.Info("Security Questions required for password reset.");
                            _askForAnswers = true;
                            return;
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
                    if (_auth.ShouldPerformDuoMFA(authSettings, LoginRequest) && !_mfaValid)
                    {
                        _duoAuthUri = await _auth.PerformDuoAuthentication(LoginRequest, "/reset");
                        PasswordResetService.AddAuthorizedRequest(_userToReset, LoginRequest.MFAToken);
                        Nav.NavigateTo(_duoAuthUri, true);
                        LoadingData = false;
                        return;
                    }
                    else if (_auth.ShouldPerformGoogleAuthenticatorMFA(appUser.Preferences, LoginRequest, authSettings) && !_mfaValid)
                    {
                        _ = googleAuthenticatorModal?.ShowAsync(_appUser?.Preferences.AuthenticatorSecret?.Decrypt().ToSecureString());
                        return;
                    }
                    else
                    {
                        _mfaValid = true;
                    }
                    if (_pinValid && _qaValid && _mfaValid)
                    {


                        _resetModel?.ShowAsync();
                    }

                }
                else
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
            }
            catch (Exception ex)
            {
                SnackBarService.Error(ex.Message);
            }
            finally
            {
                LoadingData = false;
            }
        }
        private void GoogleAuthSuccessful()
        {
            _mfaValid = true;

            _ = AttemptResetPassword();
        }
        public override void Dispose()
        {
            base.Dispose();
            _resetAuditLogger?.Dispose();
        }
    }
    class ResetAttempt
    {
        public string Username { get; set; }
        public DateTime AttemptTime { get; set; }
    }
}