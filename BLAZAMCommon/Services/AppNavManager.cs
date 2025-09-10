
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BLAZAM.Common.Services
{
    public class AppNavManager
    {
        private readonly NavigationManager _navigationManager;

        public AppNavManager(NavigationManager nav)
        {
            _navigationManager = nav;
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
    }
}
