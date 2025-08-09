
using System.Runtime.ExceptionServices;
using System.Security.Principal;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Update.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Octokit;

namespace BLAZAM.Update.Services
{
    /// <summary>
    /// Represents the source of the valid credential to write to
    /// the application directory
    /// </summary>
    public enum UpdateCredential { None, Application, Active_Directory, Custom };




    public class UpdateService : UpdateServiceBase
    {

        private readonly IStringLocalizer<AppLocalization>? AppLocalization;
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
        private readonly ApplicationInfo _applicationInfo;

        public UpdateService(ApplicationInfo applicationInfo, IAppDatabaseFactory? dbFactory = null, IStringLocalizer<AppLocalization>? appLocalization = null)
        {
            _dbFactory = dbFactory;
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
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error(ex, "An error occurred while getting latest update");
            }
            return null;

        }

        private async Task GetReleases()
        {
            //Create a GitHub client to get api data from repo

            Release? latestBranchRelease = null;

            var client = new GitHubClient(new ProductHeaderValue(Publisher_Name));




            //Get the releases from the repo
            var releases = await client.Repository.Release.GetAll(Publisher_Name, Repository_Name);
            //Filter the releases to the selected branch
            var branchReleases = releases
                .Where(r => r.TagName.Contains(SelectedBranch, StringComparison.OrdinalIgnoreCase));
            var stableReleases = releases
                .Where(r => r.TagName.Contains(ApplicationReleaseBranches.Stable, StringComparison.OrdinalIgnoreCase));
            //Get the first release,which should be the most recent
            latestBranchRelease = branchReleases.FirstOrDefault();
            //Store all other releases for use later
            AvailableUpdates.Clear();

            var betaStableReleases = releases.Where(r => r.TagName.Contains("Stable", StringComparison.OrdinalIgnoreCase));
            EncapsulateBetaReleases(betaStableReleases);
            EncapsulateStableReleases(stableReleases);
            EncapsulateLatestRelease(latestBranchRelease);
            RemoveIncompatibleReleases();

        }

        private void RemoveIncompatibleReleases()
        {
            IncompatibleUpdates = AvailableUpdates.Where(x => !x.PassesPrerequisiteChecks).ToList();
            foreach (var release in IncompatibleUpdates)
            {
                AvailableUpdates.Remove(release);
            }
        }

        private void EncapsulateLatestRelease(Release? latestBranchRelease)
        {
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
        }

        private void EncapsulateStableReleases(IEnumerable<Release> stableReleases)
        {
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
                        Loggers.UpdateLogger.Error(ex, "Error trying to get v1 releases {@Release}", release?.Name);
                    }
                }
            }
        }

        private void EncapsulateBetaReleases(IEnumerable<Release> betaStableReleases)
        {
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
                        Loggers.UpdateLogger.Error(ex, "Error trying to get beta releases {@Release}", release?.Name);
                    }
                }
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
                    var settings = await context.AppSettings.FirstAsync();
                    SelectedBranch = settings.UpdateBranch;
                    if (SelectedBranch.Equals(ApplicationReleaseBranches.Stable, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }
                    if (SelectedBranch.Equals("Stable", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SelectedBranch = ApplicationReleaseBranches.Stable;

                        settings.UpdateBranch = SelectedBranch;
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Warning(ex, "Error getting update branch from database");

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
                var update = new ApplicationUpdate(_applicationInfo, this, _dbFactory) { Release = release };
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
                Loggers.UpdateLogger.Error(ex, "Error while checking for latest update");

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

                if (ApplicationInfo.applicationRoot.Writable)
                    return UpdateCredential.Application;

                //Test Directory Credentials
                if (TestDirectoryCredentials())
                    return UpdateCredential.Active_Directory;

                // Active Directory credentials don't exist or don't have write permissions to the application directory



                //Test Update Credentials
                if (TestCustomCredentials())
                    return UpdateCredential.Custom;

                return UpdateCredential.None;
            }
        }

        public WindowsImpersonation? GetUpdateCredentials()
        {
            switch (UpdateCredential)
            {
                case UpdateCredential.Application:
                    return null;
                case UpdateCredential.Active_Directory:
                    //Pull ad settings to test if app ad account can write to the application directory
                    using (var context = _dbFactory.CreateDbContext())
                    {
                        var adSettings = context.ActiveDirectorySettings.FirstOrDefault();
                        return adSettings?.CreateDirectoryAdminImpersonator();
                    }
                case UpdateCredential.Custom:
                    using (var context2 = _dbFactory.CreateDbContext())
                    {
                        var appSettings = context2.AppSettings.FirstOrDefault();
                        return appSettings?.CreateUpdateImpersonator();
                    }
                default:
                    return null;
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

                var applicationIdentity = WindowsIdentity.GetCurrent();
                return impersonation.Run(() =>
               {
                   Loggers.UpdateLogger.Information("Checking AD update credential permissions: " + WindowsIdentity.GetCurrent().Name);
                   var impersonatedIdentity = WindowsIdentity.GetCurrent();
                   if (adSettings.Username != applicationIdentity.Name && impersonatedIdentity.Name.Equals(applicationIdentity.Name))
                   {
                       var exception = new AppException("Impersonation running as application identity");
                       ExceptionDispatchInfo.SetCurrentStackTrace(exception);
                       Loggers.ActiveDirectoryLogger.Information(exception, "Impersonation running as application identity");
                       return false;
                   }
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
