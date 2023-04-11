
using Octokit;
using BLAZAM.Common;
using BLAZAM.Update.Exceptions;
using BLAZAM.Logger;

namespace BLAZAM.Update.Services
{
    public class UpdateService : UpdateServiceBase
    {
        public ApplicationUpdate LatestUpdate { get; set; }
        public string? SelectedBranch { get; set; }

        protected readonly IHttpClientFactory httpClientFactory;
        private readonly ApplicationInfo _applicationInfo;

        public UpdateService(IHttpClientFactory _clientFactory,ApplicationInfo applicationInfo)
        {
            httpClientFactory = _clientFactory;
            _updateCheckTimer = new Timer(CheckForUpdate, null, TimeSpan.FromSeconds(20), TimeSpan.FromHours(1));
            _applicationInfo = applicationInfo;
        }

        public async Task<ApplicationUpdate?> GetLatestUpdate()
        {
            try
            {
                //var dbBranch = DatabaseCache.ApplicationSettings?.UpdateBranch;
                //if (dbBranch != null)
                //{
                //    SelectedBranch = dbBranch;
                //}

                Octokit.Release? latestRelease = null;
                ApplicationVersion? latestVer = null;


                if (SelectedBranch == null) return null;
                //Create a github client to get api data from repo
                var client = new GitHubClient(new ProductHeaderValue("BLAZAM-APP"));




                //Get the releases from the repo
                var releases = await client.Repository.Release.GetAll("Blazam-App", "Blazam");
                //Filter the releases to the selected branch
                var branchReleases = releases.Where(r => r.TagName.Contains(SelectedBranch, StringComparison.OrdinalIgnoreCase));
                //Get the first release,which should be the most recent
                latestRelease = branchReleases.FirstOrDefault();
                //Get the release filename to prepare a version object
                var filename = Path.GetFileNameWithoutExtension(latestRelease?.Assets.FirstOrDefault()?.Name);
                //Create that version object
                if (filename == null) throw new ApplicationUpdateException("Filename could not be retrieved from GitHub");
                latestVer = new ApplicationVersion(filename.Substring(filename.IndexOf("-v") + 2));




                if (latestRelease != null && latestVer != null)
                {
                    IApplicationRelease release = new ApplicationRelease
                    {
                        Branch = SelectedBranch,
                        GitHubRelease = latestRelease,
                        Version = latestVer,

                    };
                    return new ApplicationUpdate(_applicationInfo) { Release = release };

                }

            }
            catch (Octokit.RateLimitExceededException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("An error occured while getting latest update", ex);
            }
            return null;

        }

        private async void CheckForUpdate(object? state)
        {
            try
            {
                await GetLatestUpdate();
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Error while checking for latest update");
                Loggers.UpdateLogger.Error(ex.Message, ex);

            }
        }


    }
}
