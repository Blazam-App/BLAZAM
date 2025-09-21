
using BLAZAM.ActiveDirectory;
using BLAZAM.Common.Services;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Chat;
using BLAZAM.Services.Duo;

namespace BLAZAM.Gui.UI
{

    public abstract class AppComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }
        [Inject]
        protected IStringLocalizer<AppHelpLocalization> AppHelpLocalization { get; set; }

        [Inject]
        protected SearchService SearchService { get; set; }

        [Inject]
        protected PermissionApplicator PermissionApplicator { get; set; }

        [Inject]
        protected AppNavManager Nav { get; set; }

        [Inject]
        protected ConnMonitor Monitor { get; set; }

        [Inject]
        protected ApplicationInfo ApplicationInfo { get; set; }

        [Inject]
        protected IActiveDirectoryContextFactory DirectoryFactory { get; set; }

        [Inject]
        protected LockedOutUserMonitor LockedOutUserMonitor { get; set; }

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
        protected WebUserAuditLogger AuditLogger { get; set; }

        [Inject]
        protected IChatService Chat { get; set; }

        [Inject]
        protected AppDialogService MessageService { get; set; }

        [Inject]
        protected UpdateService UpdateService { get; set; }

        [Inject]
        protected AutoUpdateService AutoUpdateService { get; set; }
        [Inject]
        protected GoogleAuthenticatorService GoogleAuthenticatorService { get; set; }

        [Inject]
        protected EmailService EmailService { get; set; }

        [Inject]
        protected INotificationPublisher NotificationPublisher { get; set; }

        [Inject]
        protected IEncryptionService EncryptionService { get; set; }

        [Inject]
        protected JwtTokenService JwtTokenService { get; set; }

        [Inject]
        protected AppSnackBarService SnackBarService { get; set; }

        [Inject]
        protected Analytics Analytics { get; set; }

        private ScopedActiveDirectoryContext userActiveDirectoryService { get; set; }
        private bool _loadingData = true;
        /// <summary>
        /// Indicates whether the current component is loading data. Changing this value causes the component to re-render.
        /// </summary>
        protected bool LoadingData
        {
            get => _loadingData; set
            {
                if (_loadingData == value) return;
                _loadingData = value;
                _ = InvokeAsync(StateHasChanged);

            }
        }
        [Inject]
        protected IUserDatabaseFactory DbFactory { get; set; }


        protected override void OnInitialized()
        {
            base.OnInitialized();
            try
            {
                userActiveDirectoryService = new ScopedActiveDirectoryContext(DirectoryFactory);
                userActiveDirectoryService.Context.CurrentUser = CurrentUser.State.ToActiveDirectoryUserState();
                Directory = userActiveDirectoryService.Context;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Failed to connect to scoped active directory");

            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {

                Directory = userActiveDirectoryService.Context;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Failed to connect to scoped active directory");
            }

        }

        protected void Refresh(bool forceReload = false)
        {
            Nav.NavigateTo(Nav.Uri, forceReload);
        }
        protected void Refresh(object? state = null, object? args = null)
        {
            Nav.NavigateTo(Nav.Uri, false);
        }

        protected void InvokeStateHasChanged(object? state = null, object? args = null)
        {
            _ = StateHasChangedAsync();
        }
        /// <summary>
        /// Triggers an asynchronous update of the component's state.
        /// </summary>
        /// <remarks>This method invokes the <see cref="StateHasChanged"/> method asynchronously to notify
        /// the component that its state has changed, prompting a re-render. It is typically used to ensure the UI
        /// reflects the latest state after an asynchronous operation or external event.</remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task StateHasChangedAsync()
        {

            await InvokeAsync(StateHasChanged);
        }


        public virtual void Dispose()
        {
            this.Directory?.Dispose();
            userActiveDirectoryService?.Dispose();
        }

        /// <summary>
        /// True if the current web user is a super admin
        /// </summary>
        public bool IsAdmin { get => CurrentUser.State?.IsSuperAdmin == true; }
    }
}