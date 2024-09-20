
using BLAZAM.ActiveDirectory;
using BLAZAM.Nav;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Chat;

namespace BLAZAM.Gui.UI
{

    public class AppComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }

        [Inject]
        protected SearchService SearchService { get; set; }

        [Inject]
        protected AppNavigationManager Nav { get; set; }

        [Inject]
        protected ConnMonitor Monitor { get; set; }

        [Inject]
        protected ApplicationInfo ApplicationInfo { get; set; }

        [Inject]
        protected IActiveDirectoryContextFactory DirectoryFactory { get; set; }

        protected IActiveDirectoryContext Directory { get; set; }


        [Inject]
        protected IJSRuntime JS { get; set; }

        [Inject]
        public IApplicationUserStateService UserStateService { get; set; }

        [Inject]
        public IApplicationNewsService ApplicationNewsService { get; set; }

        [Inject]
        public ICurrentUserStateService CurrentUser { get; set; }


        [Inject]
        protected AuditLogger AuditLogger { get; set; }

        [Inject]
        protected IChatService Chat { get; set; }

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

        [Inject]
        private ScopedActiveDirectoryContext userActiveDirectoryService { get; set; }

        protected bool LoadingData { get; set; } = true;
        protected IDatabaseContext? Context;
        [Inject]
        protected IAppDatabaseFactory DbFactory { get; set; }


        protected override void OnInitialized()
        {
            base.OnInitialized();
            try
            {
                Context = DbFactory.CreateDbContext();
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error("Failed to connect to database {@Error}", ex);
            }
            try
            {

                Directory = userActiveDirectoryService.Context;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error("Failed to connect to scoped active directory {@Error}", ex);

            }
            Monitor.OnDirectoryConnectionChanged += (status) =>
            {
                InvokeAsync(StateHasChanged);
            };
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
                    Loggers.DatabaseLogger.Error("Failed to connect to database {@Error}", ex);
                }
                try
                {

                    Directory = userActiveDirectoryService.Context;
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error("Failed to connect to scoped active directory {@Error}", ex);
                }
                Monitor.OnDirectoryConnectionChanged += (status) =>
                {
                    InvokeAsync(StateHasChanged);
                };
            }
        }

        protected void Refresh()
        {
            Nav.NavigateTo(Nav.Uri, false);
        }

        protected void Refresh(bool forceReload = false)
        {
            Nav.NavigateTo(Nav.Uri, forceReload);
        }

        public virtual void Dispose()
        {
            //This object requires no further disposal
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