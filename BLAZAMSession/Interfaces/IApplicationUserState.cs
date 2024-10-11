
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Security.Claims;

namespace BLAZAM.Session.Interfaces
{
    public interface IApplicationUserState
    {
        public int Id { get; }
        AppEvent OnSettingsChanged { get; set; }

        /// <summary>
        /// Returns the combined names of the user, and if applicable, the impersonators username
        /// with the structure "{username}[ impersonated by {impersonatorName}]"
        /// </summary>
        string AuditUsername { get; }

        /// <summary>
        /// Returns the name of the user
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// The user who is impersonating this web user. It is optional, obviously.
        /// </summary>
        ClaimsPrincipal? Impersonator { get; set; }


        bool IsSuperAdmin { get; }

        /// <summary>
        /// The last request time for this web user
        /// </summary>
        DateTime LastAccessed { get; set; }

        /// <summary>
        /// The web user who is currently logged in
        /// </summary>
        ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Provides access to the user's preferences in the database
        /// </summary>
        /// <remarks>
        /// Changes made to the returned object are not saved
        /// until <see cref="SaveAllUserSettings()"/> is called
        /// </remarks>
        AppUser Preferences { get; }


        /// <summary>
        /// 
        /// </summary>
        AuthenticationTicket? Ticket { get; set; }


        //IList<UserNotification>? Notifications { get; }
        IApplicationUserSessionCache Cache { get; set; }

        string? IPAddress { get; set; }

        string LastUri { get; set; }
        bool IsAuthenticated { get; }
        List<PermissionDelegate> PermissionDelegates { get; set; }
        List<PermissionMapping> PermissionMappings { get; set; }
        List<NotificationSubscription> NotificationSubscriptions { get; set; }
        bool HasUserPrivilege { get; }
        bool HasCreateUserPrivilege { get; }
        bool HasGroupPrivilege { get; }
        bool HasCreateGroupPrivilege { get; }
        bool HasOUPrivilege { get; }
        bool HasCreateOUPrivilege { get; }
        bool HasComputerPrivilege { get; }
        bool HasBitLockerPrivilege { get; }
        bool CanUnlockUsers { get; }
        bool CanAssign { get; }
        string DuoAuthState { get; set; }

        IList<ReadNewsItem>? ReadNewsItems { get; }

        //List<ReadChatMessage> ReadChatMessages { get; }

        bool CanSearchDisabled(ActiveDirectoryObjectType objectType);
        bool Equals(object? obj);
        bool HasRole(string searchUsers);

        /// <summary>
        /// Saves the current state of the <see cref="Preferences"/> to the database
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveAllUserSettings();
        Task<bool> MarkRead(UserNotification notification);
        bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch);
        bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType);
        Task SaveDashboardWidgets();
        Task SaveReadNewsItems();
        Task SaveBasicUserPreferences();
    }
}