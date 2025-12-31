

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedEntriesWidget : TimeFrameWidget
    {
        public ChangedEntriesWidget()
        {
            Title = Localization.AppLocalization.Changed_Entries;
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
            search.Fields.Changed = DateTime.UtcNow - _timeFrame;
            ChangedEntries = (await search.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>()).Where(x => x.CanRead).ToList();
            LoadingData = false;

        }
    }
}
