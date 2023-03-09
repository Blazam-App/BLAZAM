﻿using Microsoft.VisualStudio.Services.Common;
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
        public string? SelectedBranch { get; set; }

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
                var dbBranch = DatabaseCache.ApplicationSettings?.UpdateBranch;
                if (dbBranch != null) {
                    SelectedBranch = dbBranch;
                }

                Octokit.Release? latestRelease = null;
                ApplicationVersion? latestVer = null;


                if (SelectedBranch == null) return null;
                //Create a github client to get api data from repo
                var client = new GitHubClient(new ProductHeaderValue("BLAZAM-APP"));


                if (SelectedBranch == ApplicationReleaseBranches.Dev)
                {
                    //TODO: Implemenet dev branch updates
                    HttpClient artifactClient = httpClientFactory.CreateClient();
                    artifactClient.BaseAddress = new Uri("https://api.github.com/");
                    var artifactResponse = await artifactClient.GetFromJsonAsync<ArtifactResponse>("repos/Blazam-App/BLAZAM/actions/artifacts");
                    if(artifactResponse != null && artifactResponse.artifacts.Count > 0) 
                    {
                        var latestDevArtifact = artifactResponse.artifacts.OrderBy(a => a.created_at);
                    }
                }

                else
                {
                    //Dev branch not selected, so pull the asse




                   //Get the releases from the repo
                    var releases = await client.Repository.Release.GetAll("Blazam-App", "Blazam");
                    //Filter the releases to the selected branch
                    var branchReleases = releases.Where(r => r.TagName.Contains(SelectedBranch, StringComparison.OrdinalIgnoreCase));
                    //Get the first release,which should be the most recent
                    latestRelease = branchReleases.FirstOrDefault();
                    //Get the release filename to prepare a version object
                    var filename = Path.GetFileNameWithoutExtension(latestRelease.Assets.FirstOrDefault().Name);
                    //Create that version object
                    latestVer = new ApplicationVersion(filename.Substring(filename.IndexOf("-v") + 2));


                }

                if (latestRelease != null && latestVer != null)
                {
                    IApplicationRelease release = new ApplicationRelease
                    {
                        Branch = SelectedBranch,
                        GitHubRelease = latestRelease,
                        Version = latestVer
                    };
                    return new ApplicationUpdate { Release = release };

                }
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("An error occured while getting latest update", ex);
            }
            return null;

        }

        private async void CheckForUpdate(object? state)
        {
            await GetLatestUpdate();
        }


    }
}
