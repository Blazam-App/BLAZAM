using BLAZAM.Database.Interfaces;

namespace BLAZAM.Gui.UI
{
    public class DatabaseComponentBase : AppComponentBase
    {
#nullable disable warnings
        protected IDatabaseContext Context;
        protected override async Task OnInitializedAsync()
        {
            if (Context == null)
            {
                try
                {
                    Context = await DbFactory.CreateDbContextAsync();
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error(ex, "Failed to connect to database");
                }
            }

        }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            try
            {
                Context = DbFactory.CreateDbContext();
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error(ex, "Failed to connect to database");
            }
        }
    }
}