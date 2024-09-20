
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Update.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Octokit;
using System.Diagnostics;
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
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }
        /// <summary>
        /// The latest available update for the configured <see cref="SelectedBranch"/>
        /// </summary>
        public ApplicationUpdate LatestUpdate { get; set; }
        /// <summary>
        /// All updates released under the stable branch
        /// </summary>
        public List<ApplicationUpdate> AvailableUpdates { get; set; } = new();

        /// <summary>
        /// The branch configured in the database
        /// </summary>
        public string SelectedBranch { get; set; } = ApplicationReleaseBranches.Stable;

        private const string Publisher_Name = "BLAZAM-APP";
        private const string Repository_Name = "Blazam";
        private readonly IAppDatabaseFactory? _dbFactory;
        protected readonly IHttpClientFactory httpClientFactory;
        private readonly ApplicationInfo _applicationInfo;

        public UpdateService(IHttpClientFactory _clientFactory, ApplicationInfo applicationInfo, IAppDatabaseFactory? dbFactory = null, IStringLocalizer<AppLocalization> appLocalization = null)
        {
            _dbFactory = dbFactory;
            httpClientFactory = _clientFactory;
            _applicationInfo = applicationInfo;
            AppLocalization = appLocalization;
        }
        public void Initialize()
        {
            _updateCheckTimer = new Timer(CheckForUpdate, null, TimeSpan.FromSeconds(20), TimeSpan.FromHours(1));

        }
        /// <summary>
        /// Polls GitHub for the latest release in the selected branch
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
                return NewestAvailableUpdate;

            }
            catch (RateLimitExceededException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("An error occurred while getting latest update {@Error}", ex);
            }
            return null;

        }

        private async Task GetReleases()
        {
            //Create a GitHub client to get api data from repo

            Release? latestBranchRelease = null;
            Release? latestStableRelease = null;

            var client = new GitHubClient(new ProductHeaderValue(Publisher_Name));




            //Get the releases from the repo
            var releases = await client.Repository.Release.GetAll(Publisher_Name, Repository_Name);
            //Filter the releases to the selected branch
            var branchReleases = releases.Where(r => r.TagName.Contains(SelectedBranch, StringComparison.OrdinalIgnoreCase));
            var stableReleases = releases.Where(r => r.TagName.Contains(ApplicationReleaseBranches.Stable, StringComparison.OrdinalIgnoreCase));
            //Get the first release,which should be the most recent
            latestBranchRelease = branchReleases.FirstOrDefault();
            //Store all other releases for use later
            AvailableUpdates.Clear();

            var betaStableReleases = releases.Where(r => r.TagName.Contains("Stable", StringComparison.OrdinalIgnoreCase));

            foreach (var release in betaStableReleases)
            {
                if (release != null)
                {
                    //Get the release filename to prepare a version object
                    var fn = Path.GetFileNameWithoutExtension(release?.Assets.FirstOrDefault()?.Name);
                    if (fn == null) continue;
                    //Create that update object
                    try
                    {
                        AvailableUpdates.Add(EncapsulateUpdate(release, ApplicationReleaseBranches.Stable));

                    }
                    catch (Exception ex)
                    {
                        Loggers.UpdateLogger.Error("Error trying to get beta releases {@Error}{@Release}", ex, release?.Name);
                    }
                }
            }


            foreach (var release in stableReleases)
            {
                if (release != null)
                {
                    //Get the release filename to check that the release zip exists
                    var fn = Path.GetFileNameWithoutExtension(release?.Assets.FirstOrDefault()?.Name);
                    //Create that update object
                    if (fn == null) continue;
                    try
                    {
                        AvailableUpdates.Add(EncapsulateUpdate(release, ApplicationReleaseBranches.Stable));
                    }
                    catch (Exception ex)
                    {
                        Loggers.UpdateLogger.Error("Error trying to get v1 releases {@Error}{@Release}", ex, release?.Name);
                    }
                }
            }
            if (latestBranchRelease != null)
            {
                var latestBranchUpdate = EncapsulateUpdate(latestBranchRelease, SelectedBranch);
                if (latestBranchUpdate != null && latestBranchUpdate.Branch != ApplicationReleaseBranches.Stable && latestBranchUpdate.Branch != "Stable")
                {
                    if (!AvailableUpdates.Contains(latestBranchUpdate))
                    {
                        AvailableUpdates.Add(latestBranchUpdate);
                    }

                }
            }
            IncompatibleUpdates = AvailableUpdates.Where(x => !x.PassesPrerequisiteChecks).ToList();
            foreach (var release in IncompatibleUpdates)
            {
                AvailableUpdates.Remove(release);
            }

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
                    if (SelectedBranch.Equals(ApplicationReleaseBranches.Stable, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }
                    if (SelectedBranch.Equals("Stable", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SelectedBranch = ApplicationReleaseBranches.Stable;

                        context.AppSettings.FirstOrDefault().UpdateBranch = SelectedBranch;
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Warning("Error getting update branch from database {@Error}", ex);

                }
            }

            if (SelectedBranch == null) SelectedBranch = ApplicationReleaseBranches.Stable;
        }

        private ApplicationUpdate? EncapsulateUpdate(Release? releaseToEncapsulate, string Branch)
        {
            ApplicationVersion? releaseVersion = null;

            //Get the release filename to prepare a version object
            var filename = Path.GetFileNameWithoutExtension(releaseToEncapsulate?.Assets.FirstOrDefault()?.Name);
            //Create that version object
            if (filename == null)
                throw new ApplicationUpdateException("Filename could not be retrieved from GitHub");
            releaseVersion = new ApplicationVersion(filename.Substring(filename.IndexOf("-v") + 2));





            if (releaseToEncapsulate != null && releaseVersion != null)
            {
                IApplicationRelease release = new ApplicationRelease
                {
                    Branch = Branch,
                    GitHubRelease = releaseToEncapsulate,
                    Version = releaseVersion,

                };
                var update = new ApplicationUpdate(_applicationInfo, _dbFactory) { Release = release };
                if (releaseVersion.NewerThan(new ApplicationVersion("0.9.99")))
                {
                    update.PreRequisiteChecks.Add(new(() =>
                    {
                        if (!ApplicationInfo.isUnderIIS && !PrerequisiteChecker.CheckForAspCore())
                        {
                            if (AppLocalization != null)
                                update.PrequisiteMessage = AppLocalization["ASP NET Core 8 Runtime is missing."];
                            else
                                update.PrequisiteMessage = "ASP NET Core 8 Runtime is missing.";

                            return false;

                        }
                        if (ApplicationInfo.isUnderIIS && !PrerequisiteChecker.CheckForAspCoreHosting())
                        {
                            if (AppLocalization != null)

                                update.PrequisiteMessage = AppLocalization["ASP NET Core 8 Web Hosting Bundle is missing."];
                            else
                                update.PrequisiteMessage = "ASP NET Core 8 Web Hosting Bundle is missing.";

                            return false;

                        }
                        return true;
                    }));
                }
                return update;
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
                if (TestDirectoryCredentials())
                    return UpdateCredential.Active_Directory;

                // Active Directory credentials don't exist or don't have write permissions to the application directory



                //Test Update Credentials
                if (TestCustomCredentials())
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
            if (_dbFactory == null) return false;
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

        public List<ApplicationUpdate> IncompatibleUpdates { get; private set; } = new();
        public ApplicationUpdate? NewestAvailableUpdate => AvailableUpdates.OrderByDescending(x => x.Version).FirstOrDefault();
    }
}
