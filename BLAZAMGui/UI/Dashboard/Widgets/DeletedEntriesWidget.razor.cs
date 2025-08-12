namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class DeletedEntriesWidget : Widget
    {
        public DeletedEntriesWidget()
        {
            Title = Localization.AppLocalization.Entries_deleted_in_the_last_14_days;
            WidgetType = DashboardWidgetType.DeletedEntries;
        }

        List<IDirectoryEntryAdapter> deletedObjects
        {
            get => CurrentUser.State.Cache.Get<List<IDirectoryEntryAdapter>>(this.GetType());
            set => CurrentUser.State.Cache.Set(this.GetType(), value);
        }


        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            var search = new ADSearch(Directory) { SearchRoot = Directory.GetDeleteObjectsEntry() };
            search.SearchDeleted = true;
            search.Fields.Changed = DateTime.Now.AddDays(-14);
            deletedObjects = await search.SearchAsync<DirectoryEntryAdapter, IDirectoryEntryAdapter>();
            LoadingData = false;
            LoadingData = false;

        }
    }
}
