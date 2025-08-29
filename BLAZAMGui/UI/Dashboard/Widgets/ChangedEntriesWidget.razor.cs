

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedEntriesWidget : Widget
    {
        public ChangedEntriesWidget()
        {
            Title = Localization.AppLocalization.Entries_changed_in_the_last_24_hours;
            WidgetType = DashboardWidgetType.ChangedEntries;
        }

        private List<IDirectoryEntryAdapter> ChangedEntries
        {
            get => CurrentUser.State.Cache.Get<List<IDirectoryEntryAdapter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            var search = new ADSearch(Directory);
            search.Fields.Changed = DateTime.Now.AddDays(-1);
            ChangedEntries = (await search.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>()).Where(x => x.CanRead).ToList();
            LoadingData = false;

        }
    }
}
