namespace BLAZAM.Server.Data.Services
{
    public class ApplicationManager
    {
        private IHostApplicationLifetime ApplicationLifetime { get; set; }

        public ApplicationManager(IHostApplicationLifetime applicationLifetime)
        {
            ApplicationLifetime = applicationLifetime;
        }
        public void Restart()
        {
            ApplicationLifetime.StopApplication();
        }
    }
}
