
using BLAZAM.Jobs;
using MudBlazor;

namespace BLAZAM.Gui.UI.Users
{
    public partial class UserFailedLogons : DatabaseComponentBase
    {
        private List<FailedADLogonEvent> _events = [];
        private IJob? pollingJob = null;
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
        private void OnProgressUpdated(double? progress)
        {
            InvokeStateHasChanged();
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
                    pollingJob = new Job("Polling");
                    pollingJob.OnProgressUpdated += OnProgressUpdated;
                    LockedOutUserMonitor.RecordLogonEvents(_user, pollingJob);

                    existing = Context.FailedADLogonEvents.Where(e => e.Sid.Equals(User.SID)).OrderByDescending(e => e.Timestamp).ToList();
                    _events = existing;
                    LoadingData = false;
                }
            });
          

        }

    }
}