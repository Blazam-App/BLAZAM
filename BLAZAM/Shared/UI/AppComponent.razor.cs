using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BLAZAM;
using BLAZAM.Server.Background;
using BLAZAM.Server.Shared;
using BLAZAM.Server.Pages;
using BLAZAM.Server.Shared.Layouts;
using BLAZAM.Server.Shared.Navs;
using BLAZAM.Server.Shared.Email.Base;
using BLAZAM.Server.Shared.Email;
using BLAZAM.Server.Shared.UI;
using BLAZAM.Server.Shared.UI.Users;
using BLAZAM.Server.Shared.UI.Users.Fields;
using BLAZAM.Server.Shared.UI.OU;
using BLAZAM.Server.Shared.UI.Groups;
using BLAZAM.Server.Shared.UI.Settings;
using BLAZAM.Server.Shared.UI.Settings.Templates;
using BLAZAM.Server.Shared.UI.Inputs;
using BLAZAM.Server.Shared.ResourceFiles;
using BLAZAM.Server.Shared.UI.Outputs;
using BLAZAM.Server.Shared.UI.Themes;
using BLAZAM.Server.Data;
using BLAZAM.Server.Data.Services.Update;
using BLAZAM.Server.Data.Services;
using BLAZAM.Server.Pages.Error;
using BLAZAM.Common;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.FileSystem;
using BLAZAM.Common.Exceptions;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.ActiveDirectory.Searchers;
using BLAZAM.Common.Models.Database.Audit;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models;
using BLAZAM.Common.Models.Database.Templates;
using BLAZAM.Common.Models.Database.Permissions;
using Blazorise;
using Blazorise.Extensions;
using Blazorise.Components;
using Blazorise.TreeView;
using Blazorise.LoadingIndicator;
using Blazorise.Snackbar;
using Blazorise.DataGrid;
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

        public async Task CopyToClipboard(string text)
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
            NotificationService.Info("\"" + text + "\" copied to clipboard.");
        }

        /// <summary>
        /// True if the current web user is a super admin
        /// </summary>
        public bool IsAdmin { get => UserStateService.CurrentUserState?.IsSuperAdmin == true; }
    }
}