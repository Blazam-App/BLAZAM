using Serilog;

namespace BLAZAM.Server.Background
{
    public class AutoLauncher
    {
        private Timer t;
        private IHttpClientFactory? httpClientFactory;

        public AutoLauncher(IHttpClientFactory? httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            if (httpClientFactory != null)
                t = new Timer(SendRequest, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        }

        private async void SendRequest(object? state)
        {
            Log.Information("Running Auto Launcher");
            using var httpClient = httpClientFactory.CreateClient();
            foreach (var address in Program.ListeningAddresses)
            {
                var result = await httpClient.GetAsync(address);

            }
            httpClient.Dispose();
            t.Dispose();
        }
    }
}
