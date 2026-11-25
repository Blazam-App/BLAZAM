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
            if (firstRender
                && Form != null)
            {
                await Form.ValidateAsync();
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
                        SnackBarService.Success(AppLocalization[Lang.Settings_saved]);
                    }
                    else
                    {
                        SnackBarService.Warning(AppLocalization[Lang.No_changes]);

                    }
                }
                catch (Exception ex)
                {
                    SnackBarService.Error(ex.Message);

                }
                await InvokeStateHasChangedAsync();
            }
            else
            {
                SnackBarService.Success("Database not available");

            }
        }

    }
}