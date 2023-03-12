
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using BLAZAM.Server.Background;
using BLAZAM.Server.Shared.ResourceFiles;
using BLAZAM.Server.Data.Services.Update;
using BLAZAM.Server.Data.Services;
using BLAZAM.Common;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using Blazorise;
using Microsoft.Extensions.Localization;
using BLAZAM.Server.Data.Services.Email;

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

        [Inject]
        protected IPageProgressService PageProgress { get; set; }

        [Inject]
        protected IJSRuntime JS { get; set; }

        [Inject]
        public IApplicationUserStateService UserStateService { get; set; }

        [Inject]
        protected AuditLogger AuditLogger { get; set; }

        [Inject]
        protected IMessageService MessageService { get; set; }

        [Inject]
        protected UpdateService UpdateService { get; set; }

        [Inject]
        protected AutoUpdateService AutoUpdateService { get; set; }

        [Inject]
        protected EmailService EmailService { get; set; }

        [Inject]
        protected IEncryptionService EncryptionService { get; set; }

        [Inject]
        protected INotificationService NotificationService { get; set; }

        protected bool LoadingData { get; set; } = true;
        protected DatabaseContext? Context;
        [Inject]
        protected IDbContextFactory<DatabaseContext> DbFactory { get; set; }



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

            Monitor.OnDirectoryConnectionChanged += (ConnectionState status) =>
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
            Nav.NavigateTo(Nav.Uri, true);
        }

        protected void Refresh(bool forceReload)
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
            await NotificationService.Info("\"" + text + "\" copied to clipboard.");
        }

        /// <summary>
        /// True if the current web user is a super admin
        /// </summary>
        public bool IsAdmin { get => UserStateService.CurrentUserState?.IsSuperAdmin == true; }
    }
}