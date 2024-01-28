
using BLAZAM.Localization;
using BLAZAM.Notifications.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Nav
{
    public class AppNavigationManager
    {
        NavigationManager Nav;
        private bool warnOnNavigation;

        public IJSRuntime JS { get; }
        public EventHandler<LocationChangedEventArgs> LocationChanged { get; set; }

        public bool WarnOnNavigation
        {
            get => warnOnNavigation; set
            {
                warnOnNavigation = value;
                JS.InvokeVoidAsync("window.warnOnNavigation",new object[] { warnOnNavigation });
                     }
        }


        public string ToBaseRelativePath(string uri)
        {
            return Nav.ToBaseRelativePath(uri);
        }
        public string Uri => Nav.Uri;

        public AppDialogService MessageService { get; }
        public IStringLocalizer<AppLocalization> AppLocalization { get; }

        public AppNavigationManager(IJSRuntime js, IStringLocalizer<AppLocalization> appLocalization, NavigationManager nav, AppDialogService messageService)
        {
            Nav = nav;
            JS = js;
            Nav.LocationChanged += (args, other) => { LocationChanged?.Invoke(args, other); };
            MessageService = messageService;
            AppLocalization = appLocalization;
        }

        public async void NavigateTo(string uri, bool forceLoad = false)
        {
            if (!WarnOnNavigation)
            {
                Nav.NavigateTo(uri, forceLoad);
            }
            else
            {
                if (await MessageService.Confirm(AppLocalization["Are you sure you want to navigate away?"], AppLocalization["You have unsaved changes"]) == true)
                {
                    WarnOnNavigation = false;
                    Nav.NavigateTo(uri, forceLoad);
                }
            }
        }

    }
}
