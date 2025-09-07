using MudBlazor;

namespace BLAZAM.Gui.UI.Settings
{
    public abstract class ValidatedForm : DatabaseComponentBase
    {
        protected bool SaveDisabled { get; set; }

        protected MudForm? Form;
        protected bool IsValid
        {
            get;
            set;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                Form?.Validate();
            }
        }

        protected virtual async Task Save()
        {
            if (Context != null)
            {
                try
                {
                    if (ApplicationInfo.InDemoMode && ApplicationInfo.InstallationCompleted && UserStateService.CurrentUsername.Equals("demo", StringComparison.OrdinalIgnoreCase))
                    {
                        SnackBarService.Warning("Settings changes are not allowed in the demo");
                        return;

                    }
                    var results = await Context.SaveChangesAsync();
                    if (results > 0)
                    {
                        SnackBarService.Success("Settings have been saved");
                    }
                    else
                    {
                        SnackBarService.Error(AppHelpLocalization[HelpLang.Unexpected_Error_Occurred]);

                    }
                }
                catch (Exception ex)
                {
                    SnackBarService.Error(ex.Message);

                }
                await StateHasChangedAsync();
            }
            else
            {
                SnackBarService.Success("Database not available");

            }
        }

    }
}