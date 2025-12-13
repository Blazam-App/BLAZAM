// Import necessary namespaces for various functionalities
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Global.Enums;
using BLAZAM.Gui.UI.Modals;
using BLAZAM.Gui.UI.Settings;
using BLAZAM.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime.CompilerServices;
using System.Security;

namespace BLAZAM.Pages
{
    public partial class Reset : ValidatedForm
    {
        private const string Not_Allowed_Message = "Either the user does not exist or can not reset their password";
        private GoogleAuthenticatorModal? googleAuthenticatorModal;

        [Parameter]
        public string ResetToken { get; set; }
        private string redirectUrl;
        private bool _passwordResetAvailable;
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
            _passwordResetAvailable = Context.PermissionDelegate.Any(d => d.AllowPasswordReset);
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
                var appUser = await user.GetApplicationUser(UserStateService, DbFactory, Directory,AppAuthenticationStateProvider);
                if (appUser is null)
                {
                    SnackBarService.Warning(Not_Allowed_Message);
                    return;
                }
                var eprp = new EffectivePasswordResetPolicy(appUser.PermissionDelegates);
                if (eprp.CanResetPassword)
                {
                    if (eprp.RequireEmail)
                    {
                        if (appUser.Preferences.Email.IsNullOrEmpty())
                        {
                            SnackBarService.Warning("No email configured for user");
                            return;
                        }
                        var resetToken = Guid.NewGuid();
                        appUser.Preferences.PasswordResetSettings.ResetToken = resetToken.Encrypt();
                        EmailService.SendPasswordResetEmail(appUser.Preferences.Email,"https://"+DatabaseCache.ApplicationSettings.AppFQDN+"/reset?token="+resetToken);


                        SnackBarService.Info("Verification email sent for password reset.");
                    }
                    else
                    {
                        if (eprp.RequirePIN)
                        {
                            SnackBarService.Info("PIN required for password reset.");
                        }
                        if (eprp.RequireQA)
                        {
                            SnackBarService.Info("Security Questions required for password reset.");
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