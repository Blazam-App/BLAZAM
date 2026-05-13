
using MudBlazor;

namespace BLAZAM.Gui.UI.Users
{
    public partial class UserFailedLogons : DatabaseComponentBase
    {
        private List<FailedADLogonEvent> _events = [];

        private IADUser? _user;
        [CascadingParameter]
        public IGroupableDirectoryAdapter User
        {
            get => _user; set

            {
                if (_user != null && _user.Equals(value))
                {
                    return;
                }

                if (value is IADUser adUser)
                {
                    _user = adUser;
                    LoadFailedLogons();
                }


            }
        }
        private void LoadFailedLogons()
        {
            _ = Task.Run(async () =>
            {
                if (_user != null)
                {
                    LoadingData = true;
                    _events = [.. Context.FailedADLogonEvents.Where(e => e.Sid.Equals(User.SID))];
                    _events = [.. _events.OrderByDescending(e => e.Timestamp)];

                    await StateHasChangedAsync();

                    LockedOutUserMonitor.RecordLogonEvents(_user);
                    _events = [];
                    _events = [.. Context.FailedADLogonEvents.Where(e => e.Sid.Equals(User.SID))];
                    _events = [.. _events.OrderByDescending(e => e.Timestamp)];
                    LoadingData = false;
                }

            });
        }

    }
}