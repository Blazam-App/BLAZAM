using System.Security.Claims;
using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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

        }


        private void ReloadPermissions()
        {
            var userState = _userStateService.GetUserState(_httpContextAccessor.HttpContext.User);

            if (userState != null && userState.IsAuthenticated)
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

                    if (permissiondelegate != null
                        &&
                        (permissiondelegate is IADGroup && directoryUser.IsAMemberOf(permissiondelegate as IADGroup)
                        || directoryUser.SID.ToSidString().Equals(permissiondelegate.SID.ToSidString())))
                    {
                        webUser.PermissionDelegates.Add(l);
                        webUser.PermissionMappings.AddRange(l.PermissionsMaps);
                    }

                }
#pragma warning disable S6966 // Awaitable method should be used
                if (Context.GlobalPermissionSettings.Any() && Context.GlobalPermissionSettings.First()?.AllowSelfModification == true)
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
            var selfEdit = context.GlobalPermissionSettings.Any() && context.GlobalPermissionSettings.First()?.AllowSelfModification == true;
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
                AddRoleIf(userRoles, user.HasUserPrivilege, UserRoles.SearchUsers);
                AddRoleIf(userRoles, user.HasCreateUserPrivilege, UserRoles.CreateUsers);
                AddRoleIf(userRoles, user.HasGroupPrivilege, UserRoles.SearchGroups);
                AddRoleIf(userRoles, user.HasCreateGroupPrivilege, UserRoles.CreateGroups);
                AddRoleIf(userRoles, user.HasOUPrivilege, UserRoles.SearchOUs);
                AddRoleIf(userRoles, user.HasCreateOUPrivilege, UserRoles.CreateOUs);
                AddRoleIf(userRoles, user.HasComputerPrivilege, UserRoles.SearchComputers);
                AddRoleIf(userRoles, user.HasBitLockerPrivilege, UserRoles.SearchBitLocker);
            }

            AddClaimIfNotNull(userRoles, ClaimTypes.Sid, directoryUser.SID?.ToSidString());
            AddClaimIfNotNull(userRoles, ClaimTypes.Name, directoryUser.DisplayName ?? directoryUser.SAMAccountName);
            AddClaimIfNotNull(userRoles, ClaimTypes.WindowsAccountName, directoryUser.SAMAccountName);
            AddClaimIfNotNull(userRoles, ClaimTypes.GivenName, directoryUser.GivenName);
            AddClaimIfNotNull(userRoles, ClaimTypes.Surname, directoryUser.Sn);
            AddClaimIfNotNull(userRoles, ClaimTypes.Email, directoryUser.Email);

            if (!string.IsNullOrEmpty(impersonatorSid))
            {
                userRoles.Add(new Claim(ClaimTypes.UserData, "impersonated"));
                userRoles.Add(new Claim(ClaimTypes.Actor, impersonatorSid));
            }
            else
            {
                AddClaimIfNotNull(userRoles, ClaimTypes.Actor, directoryUser.SID?.ToSidString());
            }
            return userRoles;
        }

        // Helper methods to reduce complexity
        private static void AddRoleIf(List<Claim> claims, bool condition, string role)
        {
            if (condition)
                claims.Add(new Claim(ClaimTypes.Role, role));
        }

        private static void AddClaimIfNotNull(List<Claim> claims, string type, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                claims.Add(new Claim(type, value));
        }
    }
}
