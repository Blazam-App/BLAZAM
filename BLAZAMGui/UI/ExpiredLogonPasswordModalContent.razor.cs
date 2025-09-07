using BLAZAM.Services.Events;

namespace BLAZAM.Gui.UI
{
    public partial class ExpiredLogonPasswordModalContent : AppModalContent
    {
        private string newPassword = "";
        private string newPasswordConfirm = "";


        [Parameter]
        public bool Successful { get; set; }

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
            LoadingData = true;
            await StateHasChangedAsync();
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
                        await SuccessfulChanged.InvokeAsync(Successful);
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

            LoadingData = false;
            await StateHasChangedAsync();
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