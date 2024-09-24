using BLAZAM.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Gui.UI
{
    /// <summary>
    /// Provides a generic <see cref="IDirectoryEntryAdapter"/> parameter and the modals used on view pages.
    /// <para>It also contains the search sub header and a store of custom fields</para>
    /// 
    /// <para>This is primarily geared towards search result pages</para>
    /// </summary>
    public class DirectoryEntryViewBase : DatabaseComponentBase
    {
        [Inject]
        public OUNotificationService OUNotificationService { get; set; }
        [Parameter]
        public IDirectoryEntryAdapter DirectoryEntry { get; set; }

        /// <summary>
        /// Indicates whether the current page is in edit mode.
        /// </summary>
        [Parameter]
        public bool EditMode { get; set; }

        /// <summary>
        /// A store of all custom fields defined
        /// </summary>
        protected IList<CustomActiveDirectoryField> CustomFields { get; set; } = new List<CustomActiveDirectoryField>();

        protected AppModal? UploadThumbnailModal { get; set; }
        protected AppModal? AssignToModal { get; set; }
        protected AppModal? MoveToModal { get; set; }
        protected AppModal? RenameModal { get; set; }
        protected AppModal? LogonHoursModal { get; set; }
        protected AppModal? LogOnToModal { get; set; }
        protected AppModal? ChangePasswordModal { get; set; }
        [CascadingParameter]
        protected AppModal? ChangeHistoryModal { get; set; }
        protected SetSubHeader? SubHeader { get; set; }


        /// <summary>
        /// Loads custom fields and sets up event listeners for entry changes
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (DirectoryEntry != null)
            {
                DirectoryEntry.OnModelChanged += async () =>
                {
                    await RefreshEntryComponents();
                };

                DirectoryEntry.OnDirectoryModelRenamed += Renamed;

            }
            if (Context != null)
                CustomFields = await Context.CustomActiveDirectoryFields.Where(cf => cf.DeletedAt == null).ToListAsync();
            LoadingData = false;
        }
        /// <summary>
        /// Toggles <see cref="EditMode"/>
        /// </summary>
        protected void ToggleEditMode()
        {
            EditMode = !EditMode;
        }

        private bool _savingChanges;
        protected bool SavingChanges
        {
            get => _savingChanges; set
            {
                if (_savingChanges = value) return;
                _savingChanges = value;

            }
        }
        /// <summary>
        /// Prompts the user for confirmation and sends a discard changes call to the <see cref="IDirectoryEntryAdapter"/> to remove changes if accepted
        /// </summary>
        protected virtual async void DiscardChanges()
        {
            if (await MessageService.Confirm("Are you sure you want to discard your changes?", "Discard Changes"))
            {
                DirectoryEntry.DiscardChanges();
                EditMode = false;

                Nav.WarnOnNavigation = false;

                await RefreshEntryComponents();

            }

        }

        /// <summary>
        /// Sets the expire time for <see cref="IAccountDirectoryAdapter"/> entries
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected DateTime? SetExpireTime(DateTime? time)
        {
            DateTime? expireTime = null;
            if (time != null && time != DateTime.MinValue)
                expireTime = time.Value.AddDays(1);
            return expireTime;
        }

        /// <summary>
        /// Called when an entry is renamed to update the current url
        /// </summary>
        /// <param name="renamedEntry"></param>
        protected void Renamed(IDirectoryEntryAdapter renamedEntry)
        {

            Nav.WarnOnNavigation = false;
            Nav.NavigateTo(renamedEntry.SearchUri);


        }

        /// <summary>
        /// Refreshes the <see cref="SubHeader"/> along with invoking StateHasChanged();
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshEntryComponents()
        {
            SubHeader?.Refresh();
            await InvokeAsync(StateHasChanged);
        }
    }
}
