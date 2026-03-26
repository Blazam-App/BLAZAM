using BLAZAM.Common.Data;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Update.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Octokit;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Principal;

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
        public List<ApplicationUpdate> AvailableUpdates { get; set; } = [];

        /// <summary>
        /// 
        /// </summary>
        public SystemDirectory BackupDirectory
        {
            get => new(Path.Combine(ApplicationIdentityTempDirectory.FullPath,
                "backup",
                _applicationInfo.RunningVersion.ToString()));
        }


        /// <summary>
        /// The branch configured in the database
        /// </summary>
        public string SelectedBranch { get; set; } = ApplicationReleaseBranches.Net8ReleasePrefix;

        private const string Publisher_Name = "BLAZAM-APP";
        private const string Repository_Name = "Blazam";
        private static List<string> initializedProfiles = [];
        /// <summary>
        /// Gets the temporary directory used for update operations under the application identity.
        /// </summary>
        /// <remarks>Use this method to obtain the location where temporary files related to update
        /// processes should be stored. The returned directory is accessible under the application's identity and may
        /// differ from user-specific temporary directories.</remarks>
        /// <returns>A SystemDirectory representing the temporary directory for update operations. The directory is associated
        /// with the application identity.</returns>
        public SystemDirectory GetUpdateIdentityTempDirectory()
        {

            if (OperatingSystem.IsWindows())
            {

                switch (UpdateCredential)
                {

                    case UpdateCredential.Active_Directory:
                    case UpdateCredential.Custom:
                        var identity = GetUpdateIdentity();
                        if (identity != null)
                        {
                            if (identity.ImpersonationUser !=null && !initializedProfiles.Contains(identity.ImpersonationUser.Username))
                            {
                                identity.EnsureProfileExists();
                                initializedProfiles.Add(identity.ImpersonationUser.Username);
                            }
                            return identity.Run(() =>
                            {
                                Loggers.ActiveDirectoryLogger.Information("Update Identity: {@identity}", WindowsIdentity.GetCurrent().Name);
                                //Get temp path while impersonating the update identity to ensure we have access to it
                                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                                var tempPath = Path.Combine(userProfile, "AppData", "Local", "Temp", "Blazam", "update" + Path.DirectorySeparatorChar);
                                return new SystemDirectory(tempPath);
                            });
                        }
                        break;
                }
            }
            return ApplicationIdentityTempDirectory;

        }

        public SystemDirectory ApplicationIdentityTempDirectory
        {
            get
            {
                return new SystemDirectory(_applicationInfo.TempDirectory + "update" + Path.DirectorySeparatorChar);
            }
        }
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
            var net8Releases = releases
                .Where(r => r.TagName.Contains(ApplicationReleaseBranches.Net8ReleasePrefix, StringComparison.OrdinalIgnoreCase));
            var net10Releases = releases
                .Where(r => r.TagName.Contains(ApplicationReleaseBranches.Net10ReleasePrefix, StringComparison.OrdinalIgnoreCase));
            //Get the first release,which should be the most recent
            latestBranchRelease = branchReleases.FirstOrDefault();
            //Store all other releases for use later
            lock (_updateCheckLock)
            {
                AvailableUpdates.Clear();
            }

            var betaStableReleases = releases.Where(r => r.TagName.Contains("Stable", StringComparison.OrdinalIgnoreCase));
            lock (_updateCheckLock)
            {

                EncapsulateBetaReleases(betaStableReleases);
                EncapsulateStableReleases(net8Releases);
                EncapsulateStableReleases(net10Releases, ApplicationReleaseBranches.Net10ReleasePrefix);
                EncapsulateLatestRelease(latestBranchRelease);
                RemoveIncompatibleReleases();
            }
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
                if (latestBranchUpdate != null && latestBranchUpdate.Branch != ApplicationReleaseBranches.Net8ReleasePrefix && latestBranchUpdate.Branch != "Stable")
                {
                    if (!AvailableUpdates.Contains(latestBranchUpdate))
                    {
                        AvailableUpdates.Add(latestBranchUpdate);
                    }

                }
            }
        }

        private void EncapsulateStableReleases(IEnumerable<Release> stableReleases, string applicationReleaseBranch = ApplicationReleaseBranches.Net8ReleasePrefix)
        {
            foreach (var release in stableReleases)
            {
                if (release != null)
                {
                    //Get the release filename to check that the release zip exists
                    var fn = Path.GetFileNameWithoutExtension(release?.Assets.FirstOrDefault()?.Name);
                    //Create that update object
                    if (fn == null)
                    {
                        continue;
                    }

                    try
                    {
                        AvailableUpdates.Add(EncapsulateUpdate(release, applicationReleaseBranch));
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
                    if (fn == null)
                    {
                        continue;
                    }
                    //Create that update object
                    try
                    {
                        AvailableUpdates.Add(EncapsulateUpdate(release, ApplicationReleaseBranches.Net8ReleasePrefix));

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
                    if (SelectedBranch.Equals(ApplicationReleaseBranches.Net8ReleasePrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }
                    if (SelectedBranch.Equals("Stable", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SelectedBranch = ApplicationReleaseBranches.Net8ReleasePrefix;

                        settings.UpdateBranch = SelectedBranch;
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Warning(ex, "Error getting update branch from database");

                }
            }

            if (SelectedBranch == null)
            {
                SelectedBranch = ApplicationReleaseBranches.Net8ReleasePrefix;
            }
        }

        private ApplicationUpdate? EncapsulateUpdate(Release? releaseToEncapsulate, string Branch)
        {
            if (releaseToEncapsulate == null)
            {
                return null;
            }

            var filename = Path.GetFileNameWithoutExtension(releaseToEncapsulate.Assets.FirstOrDefault()?.Name);
            if (filename == null)
            {
                throw new ApplicationUpdateException("Filename could not be retrieved from GitHub");
            }

            var versionStart = filename.IndexOf("-v");
            if (versionStart < 0 || versionStart + 2 >= filename.Length)
            {
                throw new ApplicationUpdateException("Version string not found in filename");
            }

            var releaseVersion = new ApplicationVersion(filename[(versionStart + 2)..]);

            var release = new ApplicationRelease
            {
                Branch = Branch,
                GitHubRelease = releaseToEncapsulate,
                Version = releaseVersion,
            };

            var update = new ApplicationUpdate(_applicationInfo, this, _dbFactory) { Release = release };

            if (releaseVersion.NewerThan(new ApplicationVersion("1.5.99")))
            {
                update.PreRequisiteChecks.Add(() => CheckAspCorePrerequisites(update, "10"));
            }
            else if (releaseVersion.NewerThan(new ApplicationVersion("0.9.99")))
            {
                update.PreRequisiteChecks.Add(() => CheckAspCorePrerequisites(update, "8"));
            }


            return update;
        }

        private bool CheckAspCorePrerequisites(ApplicationUpdate update, string version)
        {
            switch (version)
            {
                case "8":
                    if (!ApplicationInfo.isUnderIIS)
                    {
                        if (!PrerequisiteChecker.CheckForAspCore8())
                        {
                            update.PrequisiteMessage = AppLocalization?["ASP NET Core 8 Runtime is missing."] ?? "ASP NET Core 8 Runtime is missing.";
                            return false;
                        }
                    }
                    else
                    {
                        if (!PrerequisiteChecker.CheckForAspCoreHosting8())
                        {
                            update.PrequisiteMessage = AppLocalization?["ASP NET Core 8 Web Hosting Bundle is missing."] ?? "ASP NET Core 8 Web Hosting Bundle is missing.";
                            return false;
                        }
                    }
                    return true;

                case "10":
                    if (!ApplicationInfo.isUnderIIS)
                    {
                        if (!PrerequisiteChecker.CheckForAspCore10())
                        {
                            update.PrequisiteMessage = AppLocalization?["ASP NET Core 10 Runtime is missing."] ?? "ASP NET Core 10 Runtime is missing.";
                            return false;
                        }
                    }
                    else
                    {
                        if (!PrerequisiteChecker.CheckForAspCoreHosting10())
                        {
                            update.PrequisiteMessage = AppLocalization?["ASP NET Core 10 Web Hosting Bundle is missing."] ?? "ASP NET Core 10 Web Hosting Bundle is missing.";
                            return false;
                        }
                    }
                    return true;

            }
            return false;
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


                //Test Update Credentials
                if (TestCustomCredentials())
                {
                    return UpdateCredential.Custom;
                }

                //Test Directory Credentials
                if (TestDirectoryCredentials())
                {
                    return UpdateCredential.Active_Directory;
                }

                if (ApplicationInfo.applicationRoot.Writable && !Debugger.IsAttached)
                {
                    return UpdateCredential.Application;
                }

              




              

                return UpdateCredential.None;
            }
        }
        /// <summary>
        /// Retrieves a Windows impersonation identity used for performing update operations based on the configured
        /// update credential.
        /// </summary>
        /// <remarks>The returned impersonation identity depends on the value of UpdateCredential. If
        /// Active Directory credentials are configured and available, the method returns an impersonator for the
        /// directory admin. If custom credentials are configured, it returns an impersonator based on application
        /// settings. If neither is available, a default impersonation is returned. This method does not throw
        /// exceptions for missing or invalid credentials; callers should check the returned identity for validity
        /// before performing update operations.</remarks>
        /// <returns>A WindowsImpersonation instance representing the identity to use for updates. If no suitable identity is
        /// found, returns a default impersonation with no credentials.</returns>
        public WindowsImpersonation GetUpdateIdentity()
        {
            switch (UpdateCredential)
            {
                case UpdateCredential.Active_Directory:
                    //Pull ad settings to test if app ad account can write to the application directory
                    using (var context = _dbFactory?.CreateDbContext())
                    {
                        var adSettings = context?.ActiveDirectorySettings.FirstOrDefault();
                        return adSettings != null ? adSettings.CreateDirectoryAdminImpersonator() : new WindowsImpersonation(null);
                    }
                case UpdateCredential.Custom:
                    using (var context2 = _dbFactory?.CreateDbContext())
                    {
                        var appSettings = context2?.AppSettings.FirstOrDefault();
                        if (appSettings != null)
                        {
                            var identity = appSettings.CreateUpdateImpersonator();
                            return identity ?? new WindowsImpersonation(null);
                        }
                    }
                    break;

            }
            return new WindowsImpersonation(null);
        }
        private bool TestCustomCredentials()
        {
            using var context = _dbFactory.CreateDbContext();
            WindowsImpersonation? impersonation = null;

            var appSettings = context.AppSettings.FirstOrDefault();
            if (appSettings != null)
            {
                if (!appSettings.UseUpdateCredentials)
                {
                    return false;
                }

                impersonation = appSettings?.CreateUpdateImpersonator();

                if (impersonation != null)
                {
                    return impersonation.Run(() =>
                    {
                        Loggers.UpdateLogger.Information("Checking custom update credential permissions: " + WindowsIdentity.GetCurrent().Name);

                        if (ApplicationInfo.applicationRoot.Writable)
                        {
                            return true;
                        }

                        return false;
                    });
                }
            }
            return false;
        }

        public async Task<bool> Backup(IProgress<FileProgress>? onProgress=null)
        {
            Loggers.UpdateLogger?.Information("Attempting backup of current version to: {@BackupPath}", BackupDirectory);
            try
            {
                var result = await Task.Run(() =>
                {
                    return _applicationInfo.ApplicationRoot.CopyTo(BackupDirectory, onProgress);
                });

                Loggers.UpdateLogger?.Debug("Backup result: {@BackupResult}", result.ToString());

                return result;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger?.Error(ex, "Backup of current version failed");
                return false;
            }
        }

        private bool TestDirectoryCredentials()
        {
            if (_dbFactory == null)
            {
                return false;
            }

            using var context = _dbFactory.CreateDbContext();
            //Prepare impersonation
            WindowsImpersonation? impersonation = null;


            //Pull ad settings to test if app ad account can write to the application directory
            var adSettings = context.ActiveDirectorySettings.FirstOrDefault();
            //Make sure we got the settings
            if (adSettings != null)
            {
                impersonation = adSettings.CreateDirectoryAdminImpersonator();
            }
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
                   {
                       return true;
                   }

                   return false;

               });

            }
            return false;
        }



        public List<ApplicationUpdate> IncompatibleUpdates { get; private set; } = [];
        private readonly object _updateCheckLock = new();
        public ApplicationUpdate? NewestAvailableUpdate
        {
            get
            {
                ApplicationUpdate? latest = null;
                lock (_updateCheckLock)
                {
                    latest = AvailableUpdates.OrderByDescending(x => x.Version).FirstOrDefault();
                }
                return latest;
            }
        }
    }
}
