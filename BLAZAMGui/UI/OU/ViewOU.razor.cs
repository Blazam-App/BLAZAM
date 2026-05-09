using BLAZAM.Services.Events;
using MudBlazor;

namespace BLAZAM.Gui.UI.OU
{
    public partial class ViewOU : DirectoryEntryViewBase
    {
        private IADOrganizationalUnit? OU => DirectoryEntry as IADOrganizationalUnit;
        private IADOrganizationalUnit? parentOU;
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await StateHasChangedAsync();
            ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
            {
                EventType = ApplicationEventType.Search,
                Entry = OU,
                Actor = CurrentUser.State

            });
            if (OU != null)
            {
                parentOU = (IADOrganizationalUnit?)await OU.GetParentAsync();
            }
            LoadingData = false;
            await RefreshEntryComponents();
        }
        private async Task SaveChanges()
        {
            if (OU == null)
            {
                SnackBarService.Error("OU is null");
                return;
            }

            if (!await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes], AppLocalization[Lang.Save_Changes]))
            {
                return;
            }

            var changes = OU.Changes;
            await OU.CommitChangesAsync();
            EditMode = false;
            ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
            {
                EventType = ApplicationEventType.Modify,
                Entry = OU,
                Changes = changes,
                Actor = CurrentUser.State


            });

            SnackBarService.Success(AppLocalization["The changes made to this ou have been saved."]);

            await RefreshEntryComponents();




        }
        protected void ChildClicked(DataGridRowClickEventArgs<IDirectoryEntryAdapter> args)
        {
            if (args?.Item.SearchUri == null)
            {
                return;
            }
            Nav.NavigateTo(args.Item.SearchUri);
        }
        protected async Task DeleteOU()
        {
            if (OU == null)
            {
                return;
            }

            if (!await MessageService.Confirm("Are you sure you want to delete " + OU.CanonicalName + "?", "Delete OU"))
            {
                return;
            }

            SavingChanges = true;
            await StateHasChangedAsync();
            try
            {
                var ouName = OU.CanonicalName;
                OU.Delete();
                if (ouName != null)
                {
                    SnackBarService.Success(AppHelpLocalization[HelpLang.Deleted, ouName]);
                }
                ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                {
                    EventType = ApplicationEventType.Delete,
                    Entry = OU,
                    Actor = CurrentUser.State

                });



            }
            catch (AppException ex)
            {
                SnackBarService.Error(ex.Message);
            }

            SavingChanges = false;

            await RefreshEntryComponents();

        }
    }
}