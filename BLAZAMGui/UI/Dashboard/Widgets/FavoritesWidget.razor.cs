using BLAZAM.Localization;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class FavoritesWidget : Widget
    {
        public FavoritesWidget()
        {
            Title = AppLocalization.Favorites;
            WidgetType = DashboardWidgetType.FavoriteEntries;
        }

        List<IDirectoryEntryAdapter> FavoriteEntries
        {
            get => CurrentUser.State.Cache.Get<List<IDirectoryEntryAdapter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            await Task.Run(() =>
            {
                var favorites = new List<IDirectoryEntryAdapter>();
                foreach (var entry in CurrentUser.State.Preferences.FavoriteEntries)
                {
                    IDirectoryEntryAdapter match = Directory.GetDirectoryEntryByDN(entry.DN);
                    if (match != null && match.CanRead)
                    {
                        favorites.Add(match);
                    }

                }
                FavoriteEntries = favorites;
            });

            LoadingData = false;

        }

        protected override void RowClicked(DataGridRowClickEventArgs<IDirectoryEntryAdapter> args)
        {
            if (args.Item != null)
            {
                GoTo(args.Item);
            }
        }
    }
}
