using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using BLAZAM.Nav;
using BLAZAM.Session.Interfaces;
using DuoUniversal;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Services.Duo
{
    public interface IDuoClientProvider
    {
        public Client GetDuoClient(string callbackUri);
        public Task<bool> DoHealthCheckAsync();
    }
    public class DuoClientProvider : IDuoClientProvider
    {
        public IAppDatabaseFactory DbFactory { get; private set; }


        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string ApiHost { get; set; }
        private string RedirectUri { get; set; }

        public DuoClientProvider(IAppDatabaseFactory factory)
        {
            DbFactory = factory;
        }
        public async Task<bool> DoHealthCheckAsync()
        {
            using (var context = await DbFactory.CreateDbContextAsync())
            {
                var auth = context.AuthenticationSettings.FirstOrDefault();
                if (auth != null)
                {

                    ClientId = auth.DuoClientId;
                    ClientSecret = auth.DuoClientSecret;
                    ApiHost = auth.DuoApiHost;
                    RedirectUri = "blank";

                }
            }
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new DuoException("A 'Client ID' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new DuoException("A 'Client Secret' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ApiHost))
            {
                throw new DuoException("An 'Api Host' configuration value is required in the appsettings file.");
            }
            var client = new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri).Build();
            return await client.DoHealthCheck();
        }
        public Client GetDuoClient(string callbackUri)
        {
            using (var context = DbFactory.CreateDbContext())
            {
                var auth = context.AuthenticationSettings.FirstOrDefault();
                if (auth != null)
                {

                    ClientId = auth.DuoClientId;
                    ClientSecret = auth.DuoClientSecret;
                    ApiHost = auth.DuoApiHost;
                    RedirectUri = callbackUri;

                }
            }
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new DuoException("A 'Client ID' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new DuoException("A 'Client Secret' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ApiHost))
            {
                throw new DuoException("An 'Api Host' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(RedirectUri))
            {
                throw new DuoException("A 'Redirect URI' configuration value is required in the appsettings file.");
            }

            return new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri).Build();
        }
    }
}

