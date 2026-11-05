
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace BLAZAM.Common.Services
{
    public class AppNavManager : IDisposable
    {
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime? _jsRuntime;

        public AppNavManager(NavigationManager nav, IJSRuntime? jsRuntime = null)
        {
            _navigationManager = nav;
            _jsRuntime = jsRuntime;
            if (_navigationManager != null)
            {
                _navigationManager.LocationChanged += SendLocationChangedAnalytics;
            }
        }

        public string Uri => _navigationManager.Uri;

        public string BaseUri => _navigationManager.BaseUri;


        public event EventHandler<LocationChangedEventArgs> LocationChanged
        {
            add => _navigationManager.LocationChanged += value;
            remove => _navigationManager.LocationChanged -= value;
        }


        public string ToBaseRelativePath(string uri)
        {
            return _navigationManager.ToBaseRelativePath(uri);
        }

        public Uri ToAbsoluteUri(string relativeUri)
        {
            return _navigationManager.ToAbsoluteUri(relativeUri);
        }

        public void NavigateTo(string uri, bool forceLoad = false, bool replace = false)
        {
            _navigationManager.NavigateTo(uri, forceLoad, replace);
        }

        public void NavigateTo(string uri, NavigationOptions options)
        {
            _navigationManager.NavigateTo(uri, options);
        }


        private async void SendLocationChangedAnalytics(object? sender, LocationChangedEventArgs e)
        {
            if (_jsRuntime == null)
            {
                Loggers.SystemLogger.Warning("_jsRuntime wsa null when attempting to send location changed analytics");
            }
            else
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("trackBlazamPageView", e.Location);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Warning(ex, "Unable to send page-view analytics for navigation.");
                }
            }
        }

        public void Dispose()
        {
            if (_navigationManager != null)
            {
                _navigationManager.LocationChanged -= SendLocationChangedAnalytics;
            }
        }

    }
}
