namespace BLAZAM.Gui.UI
{
    public abstract class DatabaseComponentBase : AppComponentBase
    {
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

        public override void Dispose()
        {
            Context?.Dispose();
            base.Dispose();
        }
    }
}