
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BLAZAM.Server.Background;
using BLAZAM.Server.Shared.ResourceFiles;
using BLAZAM.Server.Data.Services.Update;
using BLAZAM.Server.Data.Services;
using BLAZAM.Common;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using Microsoft.Extensions.Localization;
using BLAZAM.Server.Data.Services.Email;
using BLAZAM.Common.Data;
using MudBlazor;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using BLAZAM.Server.Data;

namespace BLAZAM.Server.Shared.UI
{
    public class AppComponentBase:ComponentBase
    {
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }  
        
        [Inject]
        protected SearchService SearchService { get; set; }

        [Inject]
        protected IStringLocalizer<SettingsLocalization> SettingsLocalization { get; set; }

        [Inject]
        protected IStringLocalizer<UserLocalization> UserLocalization { get; set; }

        [Inject]
        protected NavigationManager Nav { get; set; }

        [Inject]
        protected ConnMonitor Monitor { get; set; }

        [Inject]
        protected IActiveDirectory Directory { get; set; }

      //TODO add page progress service

        [Inject]
        protected IJSRuntime JS { get; set; }

        [Inject]
        public IApplicationUserStateService UserStateService { get; set; }

        [Inject]
        public ICurrentUserStateService CurrentUser { get; set; }

        [Inject]
        protected AuditLogger AuditLogger { get; set; }

        [Inject]
        protected AppDialogService MessageService { get; set; }

        [Inject]
        protected UpdateService UpdateService { get; set; }

        [Inject]
        protected AutoUpdateService AutoUpdateService { get; set; }

        [Inject]
        protected EmailService EmailService { get; set; }

        [Inject]
        protected INotificationPublisher NotificationPublisher { get; set; }

        [Inject]
        protected IEncryptionService EncryptionService { get; set; }

        [Inject]
        protected AppSnackBarService SnackBarService { get; set; }

        protected bool LoadingData { get; set; } = true;
        protected IDatabaseContext? Context;
        [Inject]
        protected AppDatabaseFactory DbFactory { get; set; }



        protected override void OnInitialized()
        {
            try
            {
                Context = DbFactory.CreateDbContext();
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error("Failed to connect to database", ex);
            }

            Monitor.OnDirectoryConnectionChanged += (ServiceConnectionState status) =>
            {
                InvokeAsync(StateHasChanged);
            };
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (Context == null)
            {
                try
                {
                    Context = await DbFactory.CreateDbContextAsync();
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Failed to connect to database", ex);
                }
            }
        }

        protected void Refresh()
        {
            Nav.NavigateTo(Nav.Uri, false);
        }

        protected void Refresh(bool forceReload=false)
        {
            Nav.NavigateTo(Nav.Uri, forceReload);
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public async Task CopyToClipboard(string? text)
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
            SnackBarService.Info("\"" + text + "\" copied to clipboard.");
        }

        /// <summary>
        /// True if the current web user is a super admin
        /// </summary>
        public bool IsAdmin { get => CurrentUser.State?.IsSuperAdmin == true; }
    }
}