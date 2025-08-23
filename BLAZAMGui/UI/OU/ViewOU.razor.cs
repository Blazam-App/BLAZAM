using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Localization;
using BLAZAM.Services.Events;
using MudBlazor;

namespace BLAZAM.Gui.UI.OU
{
    public partial class ViewOU : DirectoryEntryViewBase
    {
        IADOrganizationalUnit? OU => DirectoryEntry as IADOrganizationalUnit;
        IADOrganizationalUnit? parentOU;
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InvokeAsync(StateHasChanged);
            ApplicationEvents.DirectoryEntryChanged.Invoke(new()
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
        private async void SaveChanges()
        {
            if (OU != null)
            {
                if (await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes], AppLocalization[Lang.Save_Changes]))
                {
                    var changes = OU?.Changes;
                    await OU!.CommitChangesAsync();
                    EditMode = false;
                    ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                    {
                        EventType = ApplicationEventType.Modify,
                        Entry = OU,
                        Changes = changes,
                        Actor = CurrentUser.State


                    });

                    SnackBarService.Success(AppLocalization["The changes made to this ou have been saved."]);

                    await RefreshEntryComponents();


                }
            }
            else
            {
                SnackBarService.Error("OU is null");
            }
        }
        void ChildClicked(DataGridRowClickEventArgs<IDirectoryEntryAdapter> args)
        {
            Nav.NavigateTo(args?.Item.SearchUri);
        }
        async Task DeleteOU()
        {
            if (await MessageService.Confirm("Are you sure you want to delete " + OU?.CanonicalName + "?", "Delete OU"))
            {
                SavingChanges = true;
                await InvokeAsync(StateHasChanged);
                try
                {
                    var ouName = OU.CanonicalName;
                    OU.Delete();
                    if (ouName != null)
                    {
                        SnackBarService.Success(AppHelpLocalization[HelpLang.Deleted, ouName]);
                    }
                    ApplicationEvents.DirectoryEntryChanged.Invoke(new()
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
}