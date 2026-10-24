using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using MudBlazor;

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
                ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                {
                    EventType = ApplicationEventType.Search,
                    Entry = Computer,
                    Actor = CurrentUser.State.AuditActor

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
                    ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                    {
                        EventType = ApplicationEventType.Delete,
                        Entry = Computer,
                        Actor = CurrentUser.State.AuditActor

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
            if (Computer == null || !await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes]))
            {
                return;
            }
            SavingChanges = true;
            try
            {
                var jobResults = await Computer.CommitChangesAsync();

                if (jobResults.Result == JobResult.Passed)
                {
                    EditMode = false;
                    SnackBarService.Success(AppHelpLocalization[HelpLang.Changes_Have_Been_Saved, AppLocalization[DirectoryEntry.ObjectType.ToString().ToLower()]]);
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