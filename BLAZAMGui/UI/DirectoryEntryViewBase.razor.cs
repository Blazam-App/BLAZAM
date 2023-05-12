using BLAZAM.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLAZAM.Gui.UI
{
    public class DirectoryEntryViewBase : AppComponentBase
    {
        [Parameter]
        public IDirectoryEntryAdapter DirectoryEntry { get; set; }

        protected bool EditMode { get; set; }

        protected IList<CustomActiveDirectoryField> CustomFields { get; set; } = new List<CustomActiveDirectoryField>();

        protected AppModal? UploadThumbnailModal { get; set; }
        protected AppModal? AssignToModal { get; set; }
        protected AppModal? MoveToModal { get; set; }
        protected AppModal? RenameModal { get; set; }
        protected AppModal? ChangePasswordModal { get; set; }
        protected AppModal? ChangeHistoryModal { get; set; }
        protected SetSubHeader? SubHeader { get; set; }



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

        protected void ToggleEditMode()
        {
            EditMode = !EditMode;
        }

        bool _savingChanges;
        protected bool SavingChanges
        {
            get => _savingChanges; set
            {
                if (_savingChanges = value) return;
                _savingChanges = value;

            }
        }

        protected virtual async void DiscardChanges()
        {
            if (await MessageService.Confirm("Are you sure you want to discard your changes?", "Discard Changes"))
            {
                DirectoryEntry.DiscardChanges();
                EditMode = false;
                await RefreshEntryComponents();

            }

        }

        protected DateTime? SetExpireTime(DateTime? time)
        {
            DateTime? expireTime = null;
            if (time != null && time != DateTime.MinValue)
                expireTime = time.Value.AddDays(1);
            return expireTime;
        }

        protected void Renamed(IDirectoryEntryAdapter renamedEntry)
        {


            Nav.NavigateTo(renamedEntry.SearchUri);


        }

        protected async Task RefreshEntryComponents()
        {
            SubHeader?.Refresh();
            await InvokeAsync(StateHasChanged);
        }
    }
}
