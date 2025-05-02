using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common;
using BLAZAM.Common.Data;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Server.Helpers;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BLAZAM.Services.Background
{
    public class PermissionApplicator
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        protected readonly IApplicationUserStateService _userStateService;

        protected IAppDatabaseFactory _factory { get; set; }
        protected IActiveDirectoryContext _directory { get; set; }

        public PermissionApplicator(IApplicationUserStateService userStateService, IAppDatabaseFactory factory, IActiveDirectoryContext directory, IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _userStateService = userStateService;
            _factory = factory;
            _directory = directory;

            //if (_httpContextAccessor != null)
            //{
            //    ApplicationEvents.PermissionsChanged += ReloadPermissions;
            //}
        }


        private void ReloadPermissions()
        {
            var userState = _userStateService.GetUserState(_httpContextAccessor.HttpContext.User);
            if (userState != null)
            {
                if (userState.IsAuthenticated)
                {
                    var sid = userState.Preferences?.UserGUID;
                    if (!sid.IsNullOrEmpty() && sid.StartsWith('S'))
                    {
                        var adObj = _directory.FindEntryBySID(sid.ToSidByteArray());
                        if (adObj is IADUser adUser)
                        {
                            LoadPermissions(userState, adUser);
                            var actorSid = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Actor))?.Value;
                            var userSid = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Sid))?.Value;
                            string? impersonatorSid = null;
                            if (actorSid != null && userSid != null && !actorSid.Equals(userSid))
                            {
                                impersonatorSid = actorSid;
                            }
                            var claims = TransformUserRoles(userState, adUser, impersonatorSid);
                            var identity = new ClaimsIdentity(claims, _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthenticationMethod))?.Value);

                            _httpContextAccessor.HttpContext.SignInAsync(new(identity));
                        }
                    }
                }
            }
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
#pragma warning disable S6966 // Awaitable method should be used
                if (Context.GlobalPermissionSettings.First()?.AllowSelfModification == true)
                {
                    var dbSelfAccessLevel = await Context.AccessLevels.FirstOrDefaultAsync(x => x.Name == AccessLevel.SelfAccessLevelName);
                    if (dbSelfAccessLevel != null)
                    {

                        webUser.PermissionMappings.Add(new()
                        {
                            AccessLevels = new List<AccessLevel>() { dbSelfAccessLevel },
                            Id = -1,
                            OU = directoryUser.DN
                        });
                    }

                }
#pragma warning restore S6966 // Awaitable method should be used

            }
        }



        /// <summary>
        /// Uses the Active Directory user who logged in and transforms
        /// their identity to the applications ClaimRoles based
        /// on the permissions set in the database
        /// </summary>
        /// <param name="user">The Active Directory user who authenticated</param>
        /// <returns>A list of Claim Roles that the user has been privileged</returns>
        public List<Claim> TransformUserRoles(IApplicationUserState user, IADUser directoryUser, string? impersonatorSid = null)
        {
            using var context = _factory.CreateDbContext();
            var selfEdit = context.GlobalPermissionSettings.First()?.AllowSelfModification == true;
            if (user.PermissionDelegates.Count < 1 && !selfEdit)
                throw new DeniedLoginException();

            List<Claim> userRoles = new();

            if (user.PermissionDelegates.Any(p => p.IsSuperAdmin))
            {
                userRoles.AddSuperAdmin();
                userRoles.AddAllRoles();

            }
            else
            {
                if (user.HasUserPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchUsers));
                }
                if (user.HasCreateUserPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.CreateUsers));
                }
                if (user.HasGroupPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchGroups));
                }
                if (user.HasCreateGroupPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.CreateGroups));
                }
                if (user.HasOUPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchOUs));
                }
                if (user.HasCreateOUPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.CreateOUs));
                }
                if (user.HasComputerPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchComputers));
                }
                if (user.HasBitLockerPrivilege)
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, UserRoles.SearchBitLocker));
                }
                

            }

            //TransformUserRoles returns an empty list if the user has no login rights




            if (directoryUser.DisplayName != null)
            {
                userRoles.Add(new Claim(ClaimTypes.Sid, directoryUser.SID.ToSidString()));
            }
            if (directoryUser.DisplayName != null)
            {
                userRoles.Add(new Claim(ClaimTypes.Name, directoryUser.DisplayName));
            }
            else if (directoryUser.SAMAccountName != null)
            {
                userRoles.Add(new Claim(ClaimTypes.Name, directoryUser.SAMAccountName));

            }
            if (directoryUser.UserPrincipalName != null)
                userRoles.Add(new Claim(ClaimTypes.WindowsAccountName, directoryUser.SAMAccountName));
            if (directoryUser.GivenName != null)
                userRoles.Add(new Claim(ClaimTypes.GivenName, directoryUser.GivenName));
            if (directoryUser.Sn != null)
                userRoles.Add(new Claim(ClaimTypes.Surname, directoryUser.Sn));
            if (directoryUser.Email != null)
                userRoles.Add(new Claim(ClaimTypes.Email, directoryUser.Email));

            if (impersonatorSid != null && impersonatorSid != String.Empty)
            {
                //Handle Impersonated login
                userRoles.Add(new Claim(ClaimTypes.UserData, "impersonated"));
                //Set the impersonators SID to the actor claim type so we know who to unimpersonate back to
                userRoles.Add(new Claim(ClaimTypes.Actor, impersonatorSid));

            }
            else
            {
                //This sign in is not impersonated, so we use the users SID we got from Active Directory above
                userRoles.Add(new Claim(ClaimTypes.Actor, directoryUser.SID.ToSidString()));

            }
            return userRoles;
        }

    }
}
