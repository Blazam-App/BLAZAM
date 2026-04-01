using BLAZAM.Logger;
using BLAZAM.Services.Audit;
using BLAZAM.Session;
using DuoUniversal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Claims;

namespace BLAZAM.Services.Duo
{
    public interface IDuoClientProvider
    {
        Client GetDuoClient(string callbackUri);
        Task<bool> DoHealthCheckAsync();
        Task<bool> VerifyCallback(string callbackUri,string username, string code);
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
                var auth = await context.AuthenticationSettings.FirstOrDefaultAsync();
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
        public async Task<bool> VerifyCallback(string callbackUri, string username, string code)
        {
            Client duoClient = GetDuoClient(callbackUri);

            // Get a summary of the authentication from Duo.  This will trigger an exception if the username does not match.
            try
            {
                IdToken token = await duoClient.ExchangeAuthorizationCodeFor2faResult(code, username);
                if (token.AuthResult.Result.Equals("allow", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Warning(ex, "Error attempting to perform Duo MFA");
            }
            return false;
        }
    }
}

