using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Net.Http.Headers;
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
            _updateCheckTimer = new Timer(CheckForUpdate, null, TimeSpan.FromSeconds(1), TimeSpan.FromHours(1));

        }

        public async Task<ApplicationUpdate> GetLatestUpdate()
        {
            var latestBuild = await GetLatestBuild();
            var latestVersion = await GetLatestVersion();
            var latestArtifact = await GetLatestBuildArtifact();
            if (latestBuild != null && latestVersion != null && latestArtifact != null)
                LatestUpdate = new ApplicationUpdate
                {
                    Artifact = latestArtifact,
                    Build = latestBuild,
                    Version = latestVersion
                };
            return LatestUpdate;
        }

        private async void CheckForUpdate(object state)
        {
            await GetLatestUpdate();
        }


    }
}
