
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
    public class TabbedAppComponentBase : AppComponentBase
    {
        /// <summary>
        /// The base tab uri
        /// </summary>
        /// <remarks>
        /// eg: /settings
        /// </remarks>
        protected string BaseUri = "/settings";

        [Parameter]
        public string selectedTab { get; set; } = "app";

        protected override void OnInitialized()
        {
            base.OnInitialized();
            selectedTab = selectedTab ?? "app";
        }
        protected Task OnSelectedTabChanged(string name)
        {
            selectedTab = name;
            Nav.NavigateTo(BaseUri + "/" + name);
            return Task.CompletedTask;
        }
    }
}