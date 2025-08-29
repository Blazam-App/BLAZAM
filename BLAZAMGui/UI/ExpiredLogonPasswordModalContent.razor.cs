using BLAZAM.Services.Events;
using MudBlazor;

namespace BLAZAM.Gui.UI
{
    public partial class ExpiredLogonPasswordModalContent : AppModalContent
    {
        private string newPassword = "";
        private string newPasswordConfirm = "";
        private MudSwitch<bool>? requireChangeSwitch;

        private bool _successful;

        [Parameter]
        public bool Successful
        {
            get => _successful; set
            {
                if (_successful.Equals(value)) return;
                _successful = value;
                SuccessfulChanged.InvokeAsync(value);
            }
        }

        [Parameter]
        public EventCallback<bool> SuccessfulChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Modal.SetOnYes(SaveChanges);
            Modal.SetYesText(AppLocalization[Lang.Change_Password]);

        }


        public async Task SaveChanges()
        {
            Modal.LoadingData = true;
            await InvokeAsync(StateHasChanged);
            await Task.Run(async () =>

            {
                try
                {
                    if (User.SetPassword(newPassword.ToSecureString(), false))
                    {



                        SnackBarService.Success("Your password has been updated");
                        ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.PasswordChange,
                            Entry = User,
                            Actor = CurrentUser.State


                        });
                        await InvokeAsync(() => { Successful = true; });
                        await InvokeAsync(Close);


                    }
                    else
                    {
                        SnackBarService.Error("Unable to set password for " + User.DisplayName);

                    }
                }
                catch (Exception ex)
                {
                    SnackBarService.Error(ex.Message + " " + ex.InnerException?.Message);
                }

            });

            Modal.LoadingData = false;
            await InvokeAsync(StateHasChanged);
        }

        private bool PasswordsValid
        {
            get
            {

                if (newPassword != null && newPassword != "")
                    return newPassword == newPasswordConfirm;
                return false;
            }
        }
        protected override bool IsValid => (!newPassword.IsNullOrEmpty()
                && !newPasswordConfirm.IsNullOrEmpty()
                && newPassword.Equals(newPasswordConfirm));




    }
}