// Import necessary namespaces for various functionalities
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Global.Enums;
using BLAZAM.Gui.UI.Modals;
using BLAZAM.Gui.UI.Settings;
using BLAZAM.Localization;
using Microsoft.AspNetCore.Components;
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
    public partial class Reset : ValidatedForm
    {
        private GoogleAuthenticatorModal? googleAuthenticatorModal;

        [Parameter]
        public string ResetToken { get; set; }
        private bool attemptingSignIn = false;
        private string redirectUrl;
        private bool _demoCustomLogin = false;
        private bool _passwordResetAvailable;
        private LoginRequest LoginRequest = new();

        private void SetPasswordResetAvailable()
        {
            _passwordResetAvailable = Context.PermissionDelegate.Any(d => d.AllowPasswordReset);
        }

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
            else
            {
                SetPasswordResetAvailable();
            }
        }


        private async void AppReadyChanged(ServiceConnectionState state)
        {
            if (state == ServiceConnectionState.Up)
            {
                SetPasswordResetAvailable();

                await StateHasChangedAsync();
            }


        }
      
        private bool ValidateInput(LoginRequest? validationResult)
        {
            if (validationResult == null)
            {
                validationResult = new();
            }
            if (LoginRequest.Username.IsNullOrEmpty())
            {
                validationResult.AuthenticationResult = LoginResultStatus.NoUsername;
            }

            return false;
        }
      
    }
}