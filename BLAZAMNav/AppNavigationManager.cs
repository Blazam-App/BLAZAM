
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Notifications.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace BLAZAM.Nav
{
    public class AppNavigationManager
    {
        private NavigationManager Nav;
        private bool warnOnNavigation;

        public IJSRuntime JS { get; }
        public EventHandler<LocationChangedEventArgs> LocationChanged { get; set; }

        //public bool WarnOnNavigation
        //{
        //    get => warnOnNavigation; set
        //    {
        //        warnOnNavigation = value;
        //        JS.InvokeVoidAsync("window.warnOnNavigation", new object[] { warnOnNavigation });
        //    }
        //}


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
            try
            {
                Nav.LocationChanged += (args, other) => { LocationChanged?.Invoke(args, other); };
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error trying to listen for location changes {@Error}", ex);
            }
            MessageService = messageService;
            AppLocalization = appLocalization;
        }

        public async void NavigateTo(string uri, bool forceLoad = false)
        {
            try
            {
                Nav.NavigateTo(uri, forceLoad);
            }
            catch { }
            //if (await MessageService.Confirm(AppLocalization["Are you sure you want to navigate away?"], AppLocalization["You have unsaved changes"]) == true)
            //{
            //    WarnOnNavigation = false;
            //    Nav.NavigateTo(uri, forceLoad);
            //}

        }

    }
}
