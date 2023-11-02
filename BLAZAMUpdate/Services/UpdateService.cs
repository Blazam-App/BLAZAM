
using Octokit;
using BLAZAM.Common;
using BLAZAM.Update.Exceptions;
using BLAZAM.Logger;
using BLAZAM.Helpers;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using System.Security.Principal;

namespace BLAZAM.Update.Services
{
    /// <summary>
    /// Represents the source of the valid credential to write to
    /// the application directory
    /// </summary>
    public enum UpdateCredential { None, Application, Active_Directory, Update };
    public class UpdateService : UpdateServiceBase
    {
        public ApplicationUpdate LatestUpdate { get; set; }
        public string? SelectedBranch { get; set; }

        private readonly IAppDatabaseFactory? _dbFactory;
        protected readonly IHttpClientFactory httpClientFactory;
        private readonly ApplicationInfo _applicationInfo;

        public UpdateService(IHttpClientFactory _clientFactory, ApplicationInfo applicationInfo, IAppDatabaseFactory? dbFactory = null)
        {
            _dbFactory = dbFactory;
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
                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    SelectedBranch = context.AppSettings.FirstOrDefault()?.UpdateBranch;
                }
                catch (Exception ex)
                {

                }
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
                    return new ApplicationUpdate(_applicationInfo, _dbFactory) { Release = release };

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
        public UpdateCredential UpdateCredential
        {
            get
            {
                Loggers.UpdateLogger.Information("Checking update credentials");

                if (ApplicationInfo.applicationRoot.Writable)
                    return UpdateCredential.Application;

                //Test Directory Credentials
                using var context = _dbFactory.CreateDbContext();
                //Prepare impersonation
                WindowsImpersonation impersonation = null;


                //Pull ad settings to test if app ad account can write to the application directory
                var adSettings = context.ActiveDirectorySettings.FirstOrDefault();
                //Make sure we got the settings
                if (adSettings != null)
                    impersonation = adSettings.CreateWindowsImpersonator();
                //Make sure impersonation set up and test write permissions
                if (impersonation != null && impersonation.Run(() =>
                {
                    Loggers.UpdateLogger.Information("Checking AD update credential permissions: " + WindowsIdentity.GetCurrent().Name);

                    if (ApplicationInfo.applicationRoot.Writable)
                        return true;
                    return false;
                }))
                    return UpdateCredential.Active_Directory;

                // Active Directory credentials don't exist or don't have write permissions to the application directory



                // Clear previous impersonations
                impersonation = null;
                //Test Update Credentials
                var appSettings = context.AppSettings.FirstOrDefault();
                if (appSettings != null)
                    impersonation = appSettings?.CreateWindowsImpersonator();

                if (impersonation != null && impersonation.Run(() =>
                {
                    Loggers.UpdateLogger.Information("Checking custom update credential permissions: " + WindowsIdentity.GetCurrent().Name);

                    if (ApplicationInfo.applicationRoot.Writable)
                        return true;
                    return false;
                }))
                    return UpdateCredential.Update;

                return UpdateCredential.None;
            }
        }
        public bool HasWritePermission => UpdateCredential != UpdateCredential.None;


    }
}
