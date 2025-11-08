// Import necessary namespaces for various functionalities
using BLAZAM.Common.Data;
using BLAZAM.Global.Enums;
using BLAZAM.Gui.UI.Modals;
using BLAZAM.Gui.UI.Settings;
using BLAZAM.Localization;
using Microsoft.JSInterop;
using System.Security;

namespace BLAZAM.Pages
{
    /// <summary>
    /// Represents the login functionality for the application, including user authentication, multi-factor
    /// authentication (MFA), and handling of login results.
    /// </summary>
    /// <remarks>This class provides methods to validate user credentials, process authentication results, and
    /// handle multi-factor authentication scenarios such as Google Authenticator. It also manages the application's
    /// state during the login process, including redirecting users upon successful authentication.</remarks>
    public partial class Login : ValidatedForm
    {
        private GoogleAuthenticatorModal? googleAuthenticatorModal;

        private bool attemptingSignIn = false;
        private string redirectUrl;
        private bool DemoCustomLogin = false;
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
            redirectUrl = Nav.Uri;
            LoginRequest.ReturnUrl = Nav.Uri;
            var currentUri = new Uri(Nav.Uri);
            LoginRequest.CallbackBaseUri = currentUri.Scheme + "://" + currentUri.Authority;
            if (Monitor.AppReady != ServiceConnectionState.Up)
            {
                Monitor.OnAppReadyChanged += AppReadyChanged;
            }
        }


        private async void AppReadyChanged(ServiceConnectionState state)
        {
            if (state == ServiceConnectionState.Up)
            {
                await StateHasChangedAsync();
            }


        }
        private async Task AttemptSignIn(string? otpCode = null)
        {
            attemptingSignIn = true;
            await StateHasChangedAsync();
            LoginRequest? authenticationResult = null;
            if (ValidateInput(LoginRequest))
            {
                try
                {
                    if (!otpCode.IsNullOrEmpty())
                    {
                        LoginRequest.MFAToken = otpCode;
                    }
                    var response = await JSRuntime.InvokeAsync<string>("attemptSignIn", LoginRequest);
                    authenticationResult = response.FromJson<LoginRequest>();
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error attempting logon");
                    SnackBarService.Info(ex.Message);
                }
            }

            attemptingSignIn = false;

            await ProcessAuthenticationResult(authenticationResult);

            await StateHasChangedAsync();
        }
        private bool ValidateInput(LoginRequest? validationResult)
        {
            if (validationResult == null)
            {
                validationResult = new();
            }
            if (LoginRequest.Valid || (ApplicationInfo.InDemoMode && !DemoCustomLogin))
            {
                return true;
            }

            if (LoginRequest.Password.IsNullOrEmpty())
            {
                validationResult.AuthenticationResult = LoginResultStatus.NoPassword;
            }

            if (LoginRequest.Username.IsNullOrEmpty())
            {
                validationResult.AuthenticationResult = LoginResultStatus.NoUsername;
            }

            return false;
        }
        private async Task ProcessAuthenticationResult(LoginRequest? authenticationResult = null)
        {
            if (authenticationResult == null)
            {
                return;
            }

            switch (authenticationResult.AuthenticationResult)
            {

                case LoginResultStatus.NoUsername:

                    SnackBarService.Info(AppLocalization[Lang.Username_is_missing]);
                    break;
                case LoginResultStatus.NoPassword:
                    SnackBarService.Info(AppLocalization[Lang.Password_is_missing]);
                    break;
                case LoginResultStatus.NoData:
                    SnackBarService.Warning(AppLocalization[Lang.Login_request_is_missing]);

                    break;
                case LoginResultStatus.LockedOut:
                    SnackBarService.Warning(AppLocalization[Lang.Account_is_locked_out]);

                    break;
                case LoginResultStatus.BadCredentials:
                    SnackBarService.Warning(AppLocalization[Lang.Username_or_password_not_correct]);
                    break;
                case LoginResultStatus.UnauthorizedImpersonation:
                    SnackBarService.Error(AppLocalization[Lang.Unauthorized_Impersonation_Attempt]);

                    break;
                case LoginResultStatus.DeniedLogin:
                    SnackBarService.Error(AppLocalization[Lang.You_are_not_authorized_to_login]);
                    break;
                case LoginResultStatus.UnknownFailure:
                    SnackBarService.Error(AppLocalization[Lang.Unknown_error_while_attempting_to_log_in]);

                    break;
                case LoginResultStatus.DuoRequested:
                    attemptingSignIn = true;
                    if (authenticationResult.MFARedirect != null)
                    {
                        Nav.NavigateTo(authenticationResult.MFARedirect);
                    }

                    break;
                case LoginResultStatus.GoogleAuthenticatorRequested:
                    await PerformGoogleAuthenticatorValidation(authenticationResult.MFAToken?.ToSecureString());
                    break;
                case LoginResultStatus.OK:
                    attemptingSignIn = true;
                    Nav.NavigateTo(redirectUrl, true);

                    break;
            }

        }
        private async Task PerformGoogleAuthenticatorValidation(SecureString? mfaToken)
        {
            if (googleAuthenticatorModal != null)
            {
                var passcode = await googleAuthenticatorModal.ShowAsync(mfaToken);
                if (passcode != null)
                {
                    await AttemptSignIn(passcode);
                }
                else
                {
                    attemptingSignIn = false;

                }
            }
        }

    }
}