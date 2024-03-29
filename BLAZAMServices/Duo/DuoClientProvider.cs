﻿using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using DuoUniversal;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Services.Duo
{
    public interface IDuoClientProvider
    {
        public Client GetDuoClient();
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

        public Client GetDuoClient()
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

