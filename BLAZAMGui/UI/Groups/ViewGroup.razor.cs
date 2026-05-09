using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using BLAZAM.Services.Events;
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
                Actor = CurrentUser.State

            });

            LoadingData = false;
            await RefreshEntryComponents();
        }

        private async Task SaveChanges()
        {
            if (await MessageService.Confirm("Are you sure you want to save the changes to " + Group.CanonicalName + "?", "Save Changes"))
            {


                var changes = Group.Changes;
                var assignTo = Group.MembersToAdd;
                var unassignFrom = Group.MembersToRemove;
                var jobResults = await Group.CommitChangesAsync();
                if (jobResults.Result == JobResult.Passed)
                {
                    foreach (var assignment in assignTo)
                    {
                        ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.Assign,
                            Entry = assignment.Member,
                            Target = assignment.Group,
                            Actor = CurrentUser.State

                        });

                    }

                    foreach (var assignment in unassignFrom)
                    {
                        ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.Unassign,
                            Entry = assignment.Member,
                            Target = assignment.Group,
                            Actor = CurrentUser.State

                        });

                    }
                    if (changes.Any(c => c.Field != "member"))
                    {
                        ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.Modify,
                            Entry = Group,
                            Changes = changes.Where(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName).ToList(),
                            Actor = CurrentUser.State

                        });


                    }
                    EditMode = false;
                    SnackBarService.Success("The changes made to this group have been saved.");
                }
                else
                {
                    await jobResults.ShowJobDetailsDialogAsync(MessageService);
                }
                await StateHasChangedAsync();

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
                    ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                    {
                        EventType = ApplicationEventType.Delete,
                        Entry = Group,
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