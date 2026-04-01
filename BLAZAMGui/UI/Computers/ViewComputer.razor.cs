using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using BLAZAM.Services.Events;
using MudBlazor;
using System.Runtime.CompilerServices;

namespace BLAZAM.Gui.UI.Computers
{
    public partial class ViewComputer : DirectoryEntryViewBase
    {
        private AppModal? _rebootModal;
        private IADComputer? Computer => DirectoryEntry as IADComputer;
        protected override async Task OnInitializedAsync()
        {
            Computer?.MonitorOnlineStatus();
            await base.OnInitializedAsync();
            await StateHasChangedAsync();
            if (Computer != null)
            {
                Computer.OnOnlineChanged += OnlineChanged;
            }
            if (Computer != null)
            {
                ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                {
                    EventType = ApplicationEventType.Search,
                    Entry = Computer,
                    Actor = CurrentUser.State

                });
            }
            LoadingData = false;
            await RefreshEntryComponents();
        }

        private async void OnlineChanged(bool online)
        {
            await RefreshEntryComponents();
        }
        private async Task Unlock()
        {
            if (Computer != null && await MessageService.Confirm("Are you sure you want to unlock " + Computer?.CanonicalName + "?", "Unlock Computer"))
            {
                Computer.LockedOut = false;
            }

        }
        private async Task ExpireLapsPassword()
        {
            if (Computer != null && await MessageService.Confirm("Are you sure you want to expire the LAPS password for " + Computer?.CanonicalName + "?", "Expire Password"))
            {
                Computer.LapsPasswordExpiration = DateTime.Now;
            }

        }

        private async Task DeleteComputer()
        {
            if (Computer != null && await MessageService.Confirm("Are you sure you want to delete " + Computer?.CanonicalName + "?", "Delete Computer"))
            {
                SavingChanges = true;
                await StateHasChangedAsync();
                try
                {

                    Computer.Delete(true);
                    SnackBarService.Success(Computer.CanonicalName + " has been deleted.");
                    ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                    {
                        EventType = ApplicationEventType.Delete,
                        Entry = Computer,
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
        private async Task SaveChanges()
        {
            if (Computer == null || !await MessageService.Confirm("Are you sure you want to save the changes?"))
            {
                return;
            }

            SavingChanges = true;
            await RefreshEntryComponents();

            try
            {
                var changes = Computer.Changes;
                var assignTo = Computer.ToAssignTo;
                var unassignFrom = Computer.ToUnassignFrom;
                var jobResults = await Computer.CommitChangesAsync();

                if (jobResults.Result == JobResult.Passed)
                {
                    NotifyAssignments(assignTo, ApplicationEventType.Assign);
                    NotifyAssignments(unassignFrom, ApplicationEventType.Unassign);

                    var nonMemberOfChanges = changes.Where(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName).ToList();
                    if (nonMemberOfChanges.Any())
                    {
                        ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.Modify,
                            Entry = Computer,
                            Changes = nonMemberOfChanges,
                            Actor = CurrentUser.State
                        });
                    }

                    EditMode = false;
                    SnackBarService.Success("The changes made to this computer have been saved.");
                    await RefreshEntryComponents();
                }
                else
                {
                    await jobResults.ShowJobDetailsDialogAsync(MessageService);
                }
            }
            catch (AppException ex)
            {
                SnackBarService.Error(ex.Message);
            }
            finally
            {
                SavingChanges = false;
                await RefreshEntryComponents();
            }
        }

        private void NotifyAssignments(List<GroupMembership> assignments, ApplicationEventType eventType)
        {
            foreach (var assignment in assignments)
            {
                ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                {
                    EventType = eventType,
                    Entry = assignment.Member,
                    Target = assignment.Group,
                    Actor = CurrentUser.State
                });
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Computer != null)
            {
                if (Computer.OnOnlineChanged != null)
                {
                    Computer.OnOnlineChanged -= OnlineChanged;
                }
                Computer.Dispose();
            }
        }
    }
}