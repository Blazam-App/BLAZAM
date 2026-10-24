using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using MudBlazor;

namespace BLAZAM.Gui.UI.Groups
{
    public partial class ViewGroup : DirectoryEntryViewBase
    {
        private AppModal? AssignMemberModal;



        private IADGroup Group => DirectoryEntry as IADGroup;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await StateHasChangedAsync();
            ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
            {
                EventType = ApplicationEventType.Search,
                Entry = Group,
                Actor = CurrentUser.State.AuditActor

            });

            LoadingData = false;
            await RefreshEntryComponents();
        }

        private async Task SaveChanges()
        {
            if (await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes]))
            {
                SavingChanges = true;
                var jobResults = await Group.CommitChangesAsync();
                if (jobResults.Result == JobResult.Passed)
                {
                    EditMode = false;
                    SnackBarService.Success(AppHelpLocalization[HelpLang.Changes_Have_Been_Saved, AppLocalization[DirectoryEntry.ObjectType.ToString().ToLower()]]);
                }
                else
                {
                    await jobResults.ShowJobDetailsDialogAsync(MessageService);
                }
                SavingChanges = false;

            }

        }
        private async Task DeleteGroup()
        {
            if (await MessageService.Confirm("Are you sure you want to delete " + Group.CanonicalName + "?", "Delete Group"))
            {
                SavingChanges = true;
                await StateHasChangedAsync();
                try
                {
                    Group.Delete();
                    SnackBarService.Success(Group.CanonicalName + " has been deleted.");
                }
                catch (AppException ex)
                {
                    SnackBarService.Error(ex.Message);
                }
                SavingChanges = false;
            }
        }
    }
}