using BLAZAM.Global.Data;
using BLAZAM.Plugins;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace BLAZAMGoogleWorkspacePlugin
{
    public class BlazamGoogleWorkspacePlugin : IPluginBase, IPluginServiceProvider
    {


        public string Name => "Google Workkspace Plugin";

        public PluginVersion Version => new("0.1.0");

        public string Author => "Blazam";

        public static Guid Guid => Guid.Parse("190d4931-af42-43c2-86b0-57dba331336f");

        public static IConfigurationSection Configuration { get; private set; }

        public WebApplicationBuilder InjectServices(WebApplicationBuilder builder)
        {
            Configuration = builder.Configuration.GetSection("GoogleWorkspace");
            //  var directory = EstablishConnection();



            return builder;
        }
        private async Task<DirectoryService> EstablishConnection()
        {




            ServiceAccountCredential credential;
            using (var stream = File.Open("Plugins/google.json", FileMode.Open))
            {
                credential = ServiceAccountCredential.FromServiceAccountData(stream);
            }
            credential.Scopes = ["https://www.googleapis.com/auth/admin.directory.user"];
            var token = await credential.GetAccessTokenForRequestAsync();

            var gsuiteUser = "noreply@yourdomain.com";

            var service = new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Blazam",
            });
            var user2 = await service.Users.Get("").ExecuteAsync();
            return service;
        }
    }
}
