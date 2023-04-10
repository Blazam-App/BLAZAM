using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Extensions;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Data.Services
{
    public class PermissionApplicator
    {
        protected IApplicationUserStateService _userStateService;

        protected AppDatabaseFactory _factory { get; set; }
        protected IActiveDirectoryContext _directory { get; set; }

        public PermissionApplicator(IApplicationUserStateService userStateService, AppDatabaseFactory factory, IActiveDirectoryContext directory)
        {
            _userStateService = userStateService;
            _factory = factory;
            _directory = directory;
            directory.OnNewLoginUser += LoadPermissionsForNewLoginUser;
            ProgramEvents.PermissionsChanged += PermissionsChanged;
        }

        private void PermissionsChanged()
        {
            foreach(var user in _userStateService.UserStates)
            {
                if(user.DirectoryUser != null)
                {
                    LoadPermissions(user.DirectoryUser);
                }
            }
        }

        // For every user that logs in, load their permissions
        private async void LoadPermissionsForNewLoginUser(IApplicationUserState value)
        {
            if(value.DirectoryUser!=null)
            await LoadPermissions(value.DirectoryUser);

        }

        /// <summary>
        /// Reads the current database settings and applys the assign permissions for the
        /// provided directory user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal async Task LoadPermissions(IADUser user)
        {
            using (var Context = await _factory.CreateDbContextAsync())
            {
                var cursor = await Context.PermissionDelegate.Include(pl=>pl.PermissionsMaps).ToListAsync();
                foreach(var l in cursor) { 
                
                    
                    var permissiondelegate = ActiveDirectoryContext.Instance.FindEntryBySID(l.DelegateSid);
                    if (permissiondelegate != null)
                    {
                        if ((permissiondelegate is IADGroup && user.IsAMemberOf(permissiondelegate as IADGroup))||user.SID.ToSidString().Equals(permissiondelegate.SID.ToSidString()))
                        {
                            user.PermissionDelegates.Add(l);
                            user.PermissionMappings.AddRange(l.PermissionsMaps);
                        }
                    }
                }
               
            }
        }
    }
}
