// Import necessary namespaces for various functionalities
using System.Security;
using BLAZAM.Common.Data;
using BLAZAM.Gui.UI.Modals;
using BLAZAM.Gui.UI.Settings;
using BLAZAM.Localization;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BLAZAM.Pages
{
    public partial class Login : ValidatedForm
    {
        GoogleAuthenticatorModal? googleAuthenticatorModal;

        bool attemptingSignIn = false;
        string redirectUrl;
        bool DemoCustomLogin = false;
        LoginRequest LoginRequest = new();


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            redirectUrl = Nav.Uri;
            LoginRequest.ReturnUrl = Nav.Uri;
            var currentUri = new Uri(Nav.Uri);
            LoginRequest.CallbackBaseUri = currentUri.Scheme + "://" + currentUri.Authority;
            if (Monitor.AppReady != ServiceConnectionState.Up)
                Monitor.OnAppReadyChanged += AppReadyChanged;
        }


        async void AppReadyChanged(ServiceConnectionState state)
        {
            if (state == ServiceConnectionState.Up)
                await InvokeAsync(StateHasChanged);


        }
        async Task AttemptSignIn(string? otpCode = null)
        {
            attemptingSignIn = true;
            await InvokeAsync(StateHasChanged);
            LoginRequest? authenticationResult = null;
            if (ValidateInput(LoginRequest))
                try
                {
                    if (!otpCode.IsNullOrEmpty())
                    {
                        LoginRequest.MFAToken = otpCode;
                    }
                    var response = await JSRuntime.InvokeAsync<string>("attemptSignIn", LoginRequest);
                    authenticationResult = JsonConvert.DeserializeObject<LoginRequest>(response);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error attempting logon");
                    SnackBarService.Info(ex.Message);
                }

            attemptingSignIn = false;

            await ProcessAuthenticationResult(authenticationResult);

            await InvokeAsync(StateHasChanged);
        }
        bool ValidateInput(LoginRequest? validationResult)
        {
            if (validationResult == null)
            {
                validationResult = new();
            }
            if (LoginRequest.Valid || (ApplicationInfo.InDemoMode && !DemoCustomLogin)) return true;


            if (LoginRequest.Password.IsNullOrEmpty())
                validationResult.AuthenticationResult = LoginResultStatus.NoPassword;
            if (LoginRequest.Username.IsNullOrEmpty())
                validationResult.AuthenticationResult = LoginResultStatus.NoUsername;
            return false;
        }
        async Task ProcessAuthenticationResult(LoginRequest? authenticationResult = null)
        {
            if (authenticationResult == null) return;


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
                        Nav.NavigateTo(authenticationResult.MFARedirect);
                    break;
                case LoginResultStatus.GoogleAuthenticatorRequested:
                    await PerformGoogleAuthenticatorValidation(authenticationResult.MFAToken.ToSecureString());
                    break;
                case LoginResultStatus.OK:
                    attemptingSignIn = true;

                    Nav.NavigateTo(redirectUrl, true);

                    break;
            }

        }
        async Task PerformGoogleAuthenticatorValidation(SecureString? mfaToken)
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