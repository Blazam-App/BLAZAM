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
            //directory.OnNewLoginUser += LoadPermissionsForNewLoginUser;
            //ProgramEvents.PermissionsChanged += PermissionsChanged;
        }

        //TODO Find a way to store the directory user in the userstate
        //private void PermissionsChanged()
        //{
        //    foreach (var user in _userStateService.UserStates)
        //    {
        //        if (user.DirectoryUser != null)
        //        {
        //            LoadPermissions(user.DirectoryUser);
        //        }
        //    }
        //}

        // For every user that logs in, load their permissions
        //private async void LoadPermissionsForNewLoginUser(IApplicationUserState value)
        //{
        //    if (value.DirectoryUser != null)
        //        await LoadPermissions(value.DirectoryUser);

        //}

        /// <summary>
        /// Reads the current database settings and applys the assign permissions for the
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
                    var permissiondelegate = ActiveDirectoryContext.Instance.FindEntryBySID(l.DelegateSid);
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
