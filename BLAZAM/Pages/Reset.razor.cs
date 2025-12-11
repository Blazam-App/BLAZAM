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
    public partial class Reset : ValidatedForm
    {
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

   
      
    }
}