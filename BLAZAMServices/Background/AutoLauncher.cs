using BLAZAM.Common.Data;
using Serilog;

namespace BLAZAM.Services.Background
{
    public class AutoLauncher
    {
        private Timer t;
        private ApplicationInfo info;
        private IHttpClientFactory? httpClientFactory;

        public AutoLauncher(IHttpClientFactory? httpClientFactory, ApplicationInfo info)
        {
            this.info = info;
            this.httpClientFactory = httpClientFactory;
            if (httpClientFactory != null)
                t = new Timer(SendRequest, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        }

        private async void SendRequest(object? state)
        {
            Log.Information("Running Auto Launcher");
            using var httpClient = httpClientFactory.CreateClient();
            foreach (var address in info.ListeningAddresses)
            {
                var result = await httpClient.GetAsync(address);

            }
            t.Dispose();
        }
    }
}
