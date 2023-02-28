using BLAZAM.Common.Data.Database;
using Duo;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading;

namespace BLAZAM.Server.Data.Services.Duo
{
    public interface IDuoClientProvider
    {
        public DuoClient GetDuoClient();
        public DuoApi GetApi();
    }
    internal class DuoClientProvider : IDuoClientProvider
    {
        public IDbContextFactory<DatabaseContext> DbFactory { get; private set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string ApiHost { get; set; }
        private string RedirectUri { get; set; }

        public DuoClientProvider(IDbContextFactory<DatabaseContext> factory)
        {
            DbFactory = factory;
           
        }

        public DuoClient GetDuoClient()
        {
            using (var context = DbFactory.CreateDbContext())
            {
                var auth = context.AuthenticationSettings.FirstOrDefault();
                if (auth != null)
                {

                    ClientId = auth.DuoClientId;
                    ClientSecret = auth.DuoClientSecret;
                    ApiHost = auth.DuoApiHost;
                    RedirectUri = "https://localhost/test";

                }
            }
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new ApplicationException("A 'Client ID' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new ApplicationException("A 'Client Secret' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ApiHost))
            {
                throw new ApplicationException("An 'Api Host' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(RedirectUri))
            {
                throw new ApplicationException("A 'Redirect URI' configuration value is required in the appsettings file.");
            }
           // return new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri).Build();
            return new DuoClient(ClientId, ApiHost, ClientSecret);

        }
        public DuoApi GetApi()
        {
            using (var context = DbFactory.CreateDbContext())
            {
                var auth = context.AuthenticationSettings.FirstOrDefault();
                if (auth != null)
                {

                    ClientId = auth.DuoClientId;
                    ClientSecret = auth.DuoClientSecret;
                    ApiHost = auth.DuoApiHost;
                    RedirectUri = "https://localhost/test";

                }
            }
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new ApplicationException("A 'Client ID' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new ApplicationException("A 'Client Secret' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ApiHost))
            {
                throw new ApplicationException("An 'Api Host' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(RedirectUri))
            {
                throw new ApplicationException("A 'Redirect URI' configuration value is required in the appsettings file.");
            }
           // return new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri).Build();
            return new DuoApi(ClientId, ApiHost, ClientSecret);

        }
    }
}

