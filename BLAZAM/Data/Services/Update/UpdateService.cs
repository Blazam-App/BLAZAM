using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using System.Text.Json;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Net;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using System.Collections;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.AspNetCore.Components;
using Blazorise;
using Octokit;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common;

namespace BLAZAM.Server.Data.Services.Update
{
    public class UpdateService : UpdateServiceBase
    {
        public ApplicationUpdate LatestUpdate { get; set; }
        public Task<BuildArtifact> LatestBuildArtifact { get => GetLatestBuildArtifact(); }

        protected readonly IHttpClientFactory httpClientFactory;

        public UpdateService(IHttpClientFactory _clientFactory)
        {
            httpClientFactory = _clientFactory;
            _updateCheckTimer = new Timer(CheckForUpdate, null, TimeSpan.FromSeconds(20), TimeSpan.FromHours(1));

        }

        public async Task<ApplicationUpdate> GetLatestUpdate()
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("BLAZAM-APP"));
                var releases = await client.Repository.Release.GetAll("Blazam-App", "Blazam");
                var branchReleases = releases.Where(r => r.TagName.Contains(DatabaseCache.ApplicationSettings?.UpdateBranch, StringComparison.OrdinalIgnoreCase));
                var latestRelese = branchReleases.FirstOrDefault()?.Assets.FirstOrDefault();
                var filename = Path.GetFileNameWithoutExtension(latestRelese.Name);
                var latestVer = new ApplicationVersion(filename.Substring(filename.IndexOf("-v") + 2));
                if (latestRelese != null)
                {
                    IApplicationRelease release = new ApplicationRelease
                    {
                        Branch = DatabaseCache.ApplicationSettings?.UpdateBranch,
                        DownloadURL = latestRelese.BrowserDownloadUrl,
                        ExpectedSize = latestRelese.Size,
                        Version = latestVer
                    };
                    return new ApplicationUpdate { Release = release };

                }
            }catch(Exception ex)
            {
                Loggers.UpdateLogger.Error("An error occured while getting latest update", ex);
            }
            return null;
            /*
            var latestBuild = await GetLatestBuild();
            var latestVersion = await GetLatestVersion();
            var latestArtifact = await GetLatestBuildArtifact();
            if (latestBuild != null && latestVersion != null && latestArtifact != null)
                LatestUpdate = new ApplicationUpdate
                {
                    Artifact = latestArtifact,
                    Build = latestBuild
                    //Version = latestVersion
                };
            return LatestUpdate;
            */
        }

        private async void CheckForUpdate(object? state)
        {
            await GetLatestUpdate();
        }


    }
}
