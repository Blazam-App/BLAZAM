using BLAZAM.Database.Models.User;
using BLAZAM.Jobs;
using BLAZAM.Services.Events;
using MudBlazor;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public partial class LockedOutUsers : Widget
    {
        public LockedOutUsers()
        {
            Title = string.Format(Localization.AppLocalization.Locked_Out_Users);
            WidgetType = DashboardWidgetType.LockedOutUsers;
        }

        List<IADUser> LockedUsers
        {
            get => CurrentUser.State?.Cache.Get<List<IADUser>>(this.GetType());
            set => CurrentUser.State?.Cache.Set(this.GetType(), value);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

        }

        protected override async Task RefreshDataAsync()
        {
            LoadingData = true;
            LockedUsers = (await Directory.Users.FindLockedOutUsersAsync()).OrderByDescending(u => u.LockoutTime).Where(u => u.CanRead).ToList();
            LoadingData = false;
        }

        void GoTo(DataGridRowClickEventArgs<IADUser> args)
        {
            Nav.NavigateTo(args.Item.SearchUri);
        }
        async Task UnlockAccount(IADUser userToUnlock)
        {
            if (userToUnlock.LockedOut)
            {

                userToUnlock.LockedOut = false;
                var changes = userToUnlock.Changes;

                var unlockJob = await userToUnlock.CommitChangesAsync();
                ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                {
                    EventType = ApplicationEventType.Modify,
                    Entry = userToUnlock,
                    Changes = changes,
                    Actor = CurrentUser.State

                });
                //await AuditLogger.User.Changed(userToUnlock, changes);
                if (unlockJob.Result == JobResult.Passed)
                {
                    await RefreshDataAsync();

                    SnackBarService.Success(userToUnlock.CanonicalName + " " + AppLocalization[Lang.unlocked]);
                }
                else
                {
                    SnackBarService.Error("Could not unlock: " + unlockJob.Exception?.Message);
                }
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            foreach (var entry in LockedUsers)
            {
                entry.Dispose();
            }
        }
    }
}