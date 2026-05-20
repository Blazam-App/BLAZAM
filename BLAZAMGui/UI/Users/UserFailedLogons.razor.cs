
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
            get; set;
        }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (_user == User)
            {
                return;
            }
            if (User is IADUser adUser)
            {
                _user = adUser;
                await LoadFailedLogons();
            }

        }
        private async Task LoadFailedLogons()
        {
            await Task.Run(() => {
                if (_user != null)
                {
                    LoadingData = true;
                    var existing = Context.FailedADLogonEvents.Where(e => e.Sid.Equals(_user.SID)).OrderByDescending(e => e.Timestamp).ToList();
                    _events = existing;

                    InvokeStateHasChanged();

                    LockedOutUserMonitor.RecordLogonEvents(_user);

                    existing = Context.FailedADLogonEvents.Where(e => e.Sid.Equals(User.SID)).OrderByDescending(e => e.Timestamp).ToList();
                    _events = existing;
                    LoadingData = false;
                }
            });
          

        }

    }
}