using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Search;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class ChangedEntriesWidget : Widget
    {
        public ChangedEntriesWidget()
        {
            Title = AppLocalization["Entries changed in the last 24 hours"];
            WidgetType = DashboardWidgetType.ChangedEntries;
        }

        List<IDirectoryEntryAdapter> changdEntries
        {
            get => CurrentUser.State.Cache.Get<List<IDirectoryEntryAdapter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            var search = new ADSearch(Directory);
            search.Fields.Changed = DateTime.Now.AddDays(-1);
            changdEntries = (await search.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>()).Where(x => x.CanRead).ToList();
            LoadingData = false;

        }
    }
}
