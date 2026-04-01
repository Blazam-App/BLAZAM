using BLAZAM.Database.Models;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class DeletedEntriesWidget : TimeFrameWidget
    {
        public DeletedEntriesWidget()
        {
            Title = Localization.AppLocalization.Deleted_Entries;
            WidgetType = DashboardWidgetType.DeletedEntries;
           
        }

        private List<IDirectoryEntryAdapter> DeletedEntries
        {
            get => CurrentUser.State.Cache.Get<List<IDirectoryEntryAdapter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;

            var search = new ADSearch(Directory)
            {
                SearchRoot = Directory.GetDeleteObjectsEntry(),
                SearchDeleted = true
            };
            search.Fields.Changed = DateTime.Now - _timeFrame;
            //search.Fields.Changed = DateTime.Now.AddDays(-14);
            DeletedEntries = await search.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
            LoadingData = false;

        }
    }
}
