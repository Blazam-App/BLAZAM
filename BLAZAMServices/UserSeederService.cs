using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Logger;

namespace BLAZAM.Services
{
    /// <summary>
    /// Prefills the user table with all users who have login access
    /// </summary>
    public class UserSeederService
    {
        private readonly ApplicationInfo _applicationInfo;
        private readonly IActiveDirectoryContext _activeDirectoryContext;
        private readonly IAppDatabaseFactory _dbFactory;

        public UserSeederService(IAppDatabaseFactory dbFactory, 
            IActiveDirectoryContextFactory adFactory,
            ApplicationInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
            _activeDirectoryContext = adFactory.CreateActiveDirectoryContext();
            _dbFactory = dbFactory;
            
            //TODO Move ProgramEvents to Common
            //ProgramEvents.PermissionsChanged += SeedUsers;
            SeedUsers();
        }

        private void SeedUsers()
        {
            try
            {
                EnsureAdminExists();
                if (_applicationInfo.InDemoMode)
                    EnsureDemoExists();
                using var context = _dbFactory.CreateDbContext();
                if (context.Status != Common.Data.ServiceConnectionState.Up) return;
                foreach (var deleg in context.PermissionDelegate.ToList())
                {
                    var entry = _activeDirectoryContext.FindEntryBySID(deleg.DelegateSid);
                    if (entry != null)
                    {
                        if (entry is IADUser user)
                        {
                            EnsureUserExists(user);
                        }
                        if (entry is IADGroup group)
                        {
                            foreach (var member in group.NestedMembers)
                            {
                                var type = member.GetType();
                                if (member is IADUser aduser)
                                    EnsureUserExists(aduser);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error attempting to synchronize directory and application users.", ex);
            }
        }
        /// <summary>
        /// Checks the database for this user, if not found they are added
        /// </summary>
        /// <param name="user"></param>
        private void EnsureUserExists(IADUser user)
        {
            using var context = _dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == user.SID.ToSidString()))
            {
                context.UserSettings.Add(new()
                {
                    Username = user.SamAccountName,
                    UserGUID = user.SID.ToSidString()
                });
            }
            context.SaveChanges();
        }
        /// <summary>
        /// Checks the database for this user, if not found they are added
        /// </summary>
        /// <param name="user"></param>
        private void EnsureAdminExists()
        {
            using var context = _dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == "1"))
            {
                context.UserSettings.Add(new()
                {
                    Username = "admin",
                    UserGUID = "1"
                });
            }
            context.SaveChanges();
        }
        /// <summary>
        /// Checks the database for this user, if not found they are added
        /// </summary>
        /// <param name="user"></param>
        private void EnsureDemoExists()
        {
            using var context = _dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == "2"))
            {
                context.UserSettings.Add(new()
                {
                    Username = "Demo",
                    UserGUID = "2"
                });
            }
            context.SaveChanges();
        }
    }
}
