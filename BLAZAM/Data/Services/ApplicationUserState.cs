using BLAZAM.Common.Data;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Models.Database.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BLAZAM.Server.Data.Services
{
    /// <summary>
    /// An application user state as managed by the <see cref="ApplicationUserStateService"/>
    /// </summary>
    /// <value>
    /// <para>Holds the <see cref="ClaimsPrincipal"/> of the user defined from logging
    /// in, and an impersonated claim if applied.</para>
    /// 
    /// <para>It also holds the <see cref="IADUser"/>
    /// if set, and through that, permissions can be applied.</para>
    /// </value>
    public class ApplicationUserState : IApplicationUserState
    {

        public AppEvent<AppUser> OnSettingsChange { get; set; }
        /// <summary>
        /// The web user who is currently logged in
        /// </summary>
        public ClaimsPrincipal User { get; set; }
        /// <summary>
        /// The user who is impersonating this web user. It is optional, obviously.
        /// </summary>
        public ClaimsPrincipal? Impersonator { get; set; }
        /// <summary>
        /// The <see cref="IADUser"/> which also provided the applied permissions.
        /// </summary>
        public IADUser? DirectoryUser { get; set; }
        /// <summary>
        /// The last request time for this web user
        /// </summary>
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;


        public IList<UserNotification>? Messages
        {
            get
            {
                if (!User.Identity.IsAuthenticated) return null;

                return userSettings.Messages.Where(m => !m.IsRead).ToList();
            }
        }


        public IApplicationUserSessionCache Cache { get; set; } = new ApplicationUserSessionCache();

        public AuthenticationTicket? Ticket { get; set; }


        public AppUser? userSettings { get; set; }

        private readonly INotificationPublisher _notificationPublisher;
        private readonly AppDatabaseFactory _dbFactory;

        public ApplicationUserState(AppDatabaseFactory factory, INotificationPublisher notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
            _dbFactory = factory;
            _notificationPublisher.OnNotificationPublished += ((notifications) =>
            {
                //TODO check if sent to current user
                GetUserSettingFromDB(null);
            });
        }


        /// <summary>
        /// Provides access to the user's settings in the database
        /// </summary>
        /// <remarks>
        /// Changes made to the returned object are not saved
        /// until <see cref="SaveUserSettings()"/> is called
        /// </remarks>
        public AppUser? UserSettings
        {
            get
            {
                if (!User.Identity.IsAuthenticated) return null;
                if (userSettings == null)
                {

                    GetUserSettingFromDB(null);
                }
                return userSettings;
            }
        }

        private void GetUserSettingFromDB(object? state)
        {
            try
            {
                if (User == null) return;
                using var context = _dbFactory.CreateDbContext();
                
                userSettings = context.UserSettings.Where(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid)).FirstOrDefault();
                if (userSettings == null)
                {
                    userSettings = new AppUser();
                    userSettings.UserGUID = User.FindFirstValue(ClaimTypes.Sid);
                    userSettings.Username = User.Identity?.Name;
                    context.UserSettings.Add(userSettings);
                    context.SaveChanges();

                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Saves the current state of the <see cref="UserSettings"/> to the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveUserSettings()
        {
            try
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var dbUserSettings = await context.UserSettings.Where(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid)).FirstOrDefaultAsync();
                if (dbUserSettings != null)
                {
                    dbUserSettings.Theme = this.UserSettings?.Theme;
                    dbUserSettings.SearchDisabledUsers = this.UserSettings.SearchDisabledUsers;
                    dbUserSettings.SearchDisabledComputers = this.UserSettings.SearchDisabledComputers;
                    OnSettingsChange?.Invoke(dbUserSettings);

                    return (await context.SaveChangesAsync()) > 0;
                }

            }
            catch
            {

            }
            return false;
        }
        public bool IsSuperAdmin
        {
            get
            {
                if (User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin)) return true;
                if (DirectoryUser != null)
                    return DirectoryUser.PermissionDelegates.Any(p => p.IsSuperAdmin);
                return false;
            }
        }
        /// <summary>
        /// Returns the name of the user
        /// </summary>
        public string? Username
        {
            get
            {
                string? auditUsername = User.Identity?.Name;

                return auditUsername;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                try
                {
                    return User.Identity.IsAuthenticated;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns the combined names of the user, and if applicable, the impersonators username
        /// with the structure "{username}[ impersonated by {impersonatorName}]"
        /// </summary>
        public string? AuditUsername
        {
            get
            {
                string? auditUsername = Username;
                if (Impersonator != null)
                {
                    auditUsername += " impersonated by " + Impersonator?.Identity?.Name;
                }
                return auditUsername;
            }
        }

        public string LastUri { get; set; }

        public override int GetHashCode()
        {
            return User.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is ApplicationUserState otherState)
            {

                if (otherState.User.FindFirstValue(ClaimTypes.Sid) == this.User.FindFirstValue(ClaimTypes.Sid)
                    && otherState.User.FindFirstValue(ClaimTypes.Actor) == User.FindFirstValue(ClaimTypes.Actor))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasRole(string userRole)
        {
            return User.HasClaim(ClaimTypes.Role, userRole);
        }
    }
}