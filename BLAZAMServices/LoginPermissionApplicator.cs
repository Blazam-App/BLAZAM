using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Interfaces;

using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Services
{
    public class PermissionApplicator
    {
        protected IApplicationUserStateService _userStateService;

        protected IAppDatabaseFactory _factory { get; set; }
        protected IActiveDirectoryContext _directory { get; set; }

        public PermissionApplicator(IApplicationUserStateService userStateService, IAppDatabaseFactory factory, IActiveDirectoryContext directory)
        {
            _userStateService = userStateService;
            _factory = factory;
            _directory = directory;
        }

      

      

        /// <summary>
        /// Reads the current database settings and applies the assign permissions for the
        /// provided directory user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task LoadPermissions(IApplicationUserState webUser, IADUser directoryUser)
        {
            using (var Context = await _factory.CreateDbContextAsync())
            {
                var cursor = await Context.PermissionDelegate.Include(pl => pl.PermissionsMaps).ToListAsync();
                foreach (var l in cursor)
                {
                    var permissiondelegate = ActiveDirectoryContext.SystemInstance.FindEntryBySID(l.DelegateSid);
                    if (permissiondelegate != null)
                    {
                        if (permissiondelegate is IADGroup && directoryUser.IsAMemberOf(permissiondelegate as IADGroup)
                            || directoryUser.SID.ToSidString().Equals(permissiondelegate.SID.ToSidString()))
                        {
                            webUser.PermissionDelegates.Add(l);
                            webUser.PermissionMappings.AddRange(l.PermissionsMaps);
                        }
                    }
                }

            }
        }
    }
}
