
using Microsoft.AspNetCore.Components.Routing;


namespace BLAZAM.Server
{
    public partial class AppUserPageTracker
    {
        string _lastUri;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Nav.LocationChanged += TrackNavigation;
        }

        private void TrackNavigation(object? sender, LocationChangedEventArgs e)
        {
            try
            {
                if (Nav.Uri != _lastUri)
                {
                    UserStateService.CurrentUserState.LastUri = Nav.ToBaseRelativePath(Nav.Uri);
                    _lastUri = Nav.Uri;
                }

            }
            catch
            {

            }
        }
    }
}