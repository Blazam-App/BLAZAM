

using BLAZAM.Common.Data.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using System.Reflection.PortableExecutable;

namespace BLAZAM.Server.Shared.UI.Settings
{
    public partial class SettingsComponents:ValidatedForm
    {
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }
        protected void Save(IEnumerable<EntityEntry> changedEntries)
        {
            base.Save();
        }
    }
}