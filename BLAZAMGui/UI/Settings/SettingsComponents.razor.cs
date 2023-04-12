using BLAZAM.Gui.UI.Settings;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BLAZAM.Gui.UI.Settings
{
    public partial class SettingsComponents:ValidatedForm
    {
        protected object? originalSettings;

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }
        //TODO do we need this save
        protected void Save(IEnumerable<EntityEntry> changedEntries)
        {
     
            base.Save();
        }
    }
}