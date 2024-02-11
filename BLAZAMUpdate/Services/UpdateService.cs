
using Octokit;
using BLAZAM.Common;
using BLAZAM.Update.Exceptions;
using BLAZAM.Logger;
using BLAZAM.Helpers;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using System.Security.Principal;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;

namespace BLAZAM.Update.Services
{
    /// <summary>
    /// Represents the source of the valid credential to write to
    /// the application directory
    /// </summary>
    public enum UpdateCredential { None, Application, Active_Directory, Update };




    public class UpdateService : UpdateServiceBase
    {

        /// <summary>
        /// The latest available update for the configured <see cref="SelectedBranch"/>
        /// </summary>
        public ApplicationUpdate LatestUpdate { get; set; }
        /// <summary>
        /// All updates released under the stable branch
        /// </summary>
        public List<ApplicationUpdate> StableUpdates { get; set; } = new();

        /// <summary>
        /// The branch configured in the database
        /// </summary>
        public string SelectedBranch { get; set; } = ApplicationReleaseBranches.Stable;

        private const string Publisher_Name = "BLAZAM-APP";
        private const string Repository_Name = "Blazam";
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
        /// <summary>
        /// Polls Github for the latest release in the selected branch
        /// </summary>
        /// <remarks>
        /// Also collects all stable releases for changelogs.
        /// </remarks>
        /// <returns>A task that will return the latest stable <see cref="ApplicationUpdate"/> if it is reachable </returns>
        /// <exception cref="ApplicationUpdateException"></exception>
        public async Task<ApplicationUpdate?> GetUpdates()
        {
            try
            {

                await SetBranch();
                await GetReleases();
                return LatestUpdate;

            }
            catch (Octokit.RateLimitExceededException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("An error occured while getting latest update {@Error}", ex);
            }
            return null;

        }

        private async Task GetReleases()
        {
            //Create a github client to get api data from repo

            Octokit.Release? latestRelease = null;

            var client = new GitHubClient(new ProductHeaderValue(Publisher_Name));




            //Get the releases from the repo
            var releases = await client.Repository.Release.GetAll(Publisher_Name, Repository_Name);
            //Filter the releases to the selected branch
            var branchReleases = releases.Where(r => r.TagName.Contains(SelectedBranch, StringComparison.OrdinalIgnoreCase));
            var stableReleases = releases.Where(r => r.TagName.Contains(ApplicationReleaseBranches.Stable, StringComparison.OrdinalIgnoreCase));
            //Get the first release,which should be the most recent
            latestRelease = branchReleases.FirstOrDefault();
            //Store all other releases for use later
            StableUpdates.Clear();
            foreach (var release in stableReleases)
            {
                //Get the release filename to prepare a version object
                var fn = Path.GetFileNameWithoutExtension(release?.Assets.FirstOrDefault()?.Name);
                //Create that version object
                if (fn == null) continue;
                StableUpdates.Add(EncapsulateUpdate(release, ApplicationReleaseBranches.Stable));

            }
            LatestUpdate = EncapsulateUpdate(latestRelease, SelectedBranch);
        }

        /// <summary>
        /// Sets the branch based on the value in the database
        /// </summary>
        /// <returns>The configured branch from the database, if database is unreachable, Stable</returns>
        private async Task SetBranch()
        {
            //Set the branch, if the db is unreachable use Stable
            if (_dbFactory != null)
            {

                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    SelectedBranch = context.AppSettings.FirstOrDefault()?.UpdateBranch;
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Error getting update branch from database {@Error}", ex);

                }
            }

            if (SelectedBranch == null) SelectedBranch = ApplicationReleaseBranches.Stable;
        }

        private ApplicationUpdate? EncapsulateUpdate(Release? latestRelease, string Branch)
        {
            ApplicationVersion? latestVer = null;

            //Get the release filename to prepare a version object
            var filename = Path.GetFileNameWithoutExtension(latestRelease?.Assets.FirstOrDefault()?.Name);
            //Create that version object
            if (filename == null) throw new ApplicationUpdateException("Filename could not be retrieved from GitHub");
            latestVer = new ApplicationVersion(filename.Substring(filename.IndexOf("-v") + 2));




            if (latestRelease != null && latestVer != null)
            {
                IApplicationRelease release = new ApplicationRelease
                {
                    Branch = Branch,
                    GitHubRelease = latestRelease,
                    Version = latestVer,

                };
                return new ApplicationUpdate(_applicationInfo, _dbFactory) { Release = release };

            }
            return null;
        }

        private async void CheckForUpdate(object? state)
        {
            try
            {
                await GetUpdates();
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Error while checking for latest update {@Error}", ex);

            }
        }

        /// <summary>
        /// The type of credential validated to be able to write to the app directory
        /// </summary>
        public UpdateCredential UpdateCredential
        {
            get
            {
                Loggers.UpdateLogger.Information("Checking update credentials");

                if (!Debugger.IsAttached && ApplicationInfo.applicationRoot.Writable)
                    return UpdateCredential.Application;

                //Test Directory Credentials
                if (!Debugger.IsAttached && TestDirectoryCredentials())
                    return UpdateCredential.Active_Directory;

                // Active Directory credentials don't exist or don't have write permissions to the application directory



                //Test Update Credentials
               if(TestCustomCredentials())
                    return UpdateCredential.Update;

                return UpdateCredential.None;
            }
        }

        private bool TestCustomCredentials()
        {
            using var context = _dbFactory.CreateDbContext();
            WindowsImpersonation? impersonation = null;

            var appSettings = context.AppSettings.FirstOrDefault();
            if (appSettings != null)
            {
                if (!appSettings.UseUpdateCredentials) return false;
                impersonation = appSettings?.CreateUpdateImpersonator();

                if (impersonation != null)
                {
                    return impersonation.Run(() =>
                    {
                        Loggers.UpdateLogger.Information("Checking custom update credential permissions: " + WindowsIdentity.GetCurrent().Name);

                        if (ApplicationInfo.applicationRoot.Writable)
                            return true;
                        return false;
                    });
                }
            }
            return false;
        }

        private bool TestDirectoryCredentials()
        {
            using var context = _dbFactory.CreateDbContext();
            //Prepare impersonation
            WindowsImpersonation? impersonation = null;


            //Pull ad settings to test if app ad account can write to the application directory
            var adSettings = context.ActiveDirectorySettings.FirstOrDefault();
            //Make sure we got the settings
            if (adSettings != null)
                impersonation = adSettings.CreateDirectoryAdminImpersonator();
            //Make sure impersonation set up and test write permissions
            if (impersonation != null)
            {

                return impersonation.Run(() =>
               {
                   Loggers.UpdateLogger.Information("Checking AD update credential permissions: " + WindowsIdentity.GetCurrent().Name);

                   if (ApplicationInfo.applicationRoot.Writable)
                       return true;
                   return false;

               });

            }
            return false;
        }

        /// <summary>
        /// Returns true if any configured credentials have write permission to the app directory
        /// </summary>
        public bool HasWritePermission => UpdateCredential != UpdateCredential.None;


    }
}
