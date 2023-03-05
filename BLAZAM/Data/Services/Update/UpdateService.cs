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
                var selectedBranch = DatabaseCache.ApplicationSettings?.UpdateBranch;

                Octokit.Release? latestRelease = null;
                ApplicationVersion? latestVer = null;


                if (selectedBranch == null) throw new ApplicationUpdateException("There was no branch selection found in the database");

                //Create a github client to get api data from repo
                var client = new GitHubClient(new ProductHeaderValue("BLAZAM-APP"));


                if (selectedBranch == ApplicationReleaseBranches.Dev)
                {
                   //TODO: Implemenet dev branch updates

                }

                else
                {
                    //Dev branch not selected, so pull the asse




                   //Get the releases from the repo
                    var releases = await client.Repository.Release.GetAll("Blazam-App", "Blazam");
                    //Filter the releases to the selected branch
                    var branchReleases = releases.Where(r => r.TagName.Contains(selectedBranch, StringComparison.OrdinalIgnoreCase));
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
                        Branch = selectedBranch,
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
