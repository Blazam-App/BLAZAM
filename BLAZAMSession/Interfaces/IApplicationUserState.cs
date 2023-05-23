
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BLAZAM.Session.Interfaces
{
    public interface IApplicationUserState
    {
        public int Id { get; }
        AppEvent<AppUser> OnSettingsChanged { get; set; }

        /// <summary>
        /// Returns the combined names of the user, and if applicable, the impersonators username
        /// with the structure "{username}[ impersonated by {impersonatorName}]"
        /// </summary>
        string AuditUsername { get; }

        /// <summary>
        /// Returns the name of the user
        /// </summary>
        string Username { get; }

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
        /// until <see cref="SaveUserSettings()"/> is called
        /// </remarks>
        AppUser Preferences { get; }


        /// <summary>
        /// 
        /// </summary>
        AuthenticationTicket? Ticket { get; set; }


        IList<UserNotification>? Notifications { get; }
        IApplicationUserSessionCache Cache { get; set; }


        string LastUri { get; set; }
        bool IsAuthenticated { get; }
        List<PermissionDelegate> PermissionDelegates { get; set; }
        List<PermissionMapping> PermissionMappings { get; set; }
        bool HasUserPrivilege { get; }
        bool HasCreateUserPrivilege { get; }
        bool HasGroupPrivilege { get; }
        bool HasCreateGroupPrivilege { get; }
        bool HasOUPrivilege { get; }
        bool HasCreateOUPrivilege { get; }
        bool HasComputerPrivilege { get; }
        bool CanUnlockUsers { get;  }
        //List<ReadChatMessage> ReadChatMessages { get; }

        bool CanSearchDisabled(ActiveDirectoryObjectType objectType);
        bool Equals(object? obj);
        bool HasRole(string searchUsers);

        /// <summary>
        /// Saves the current state of the <see cref="Preferences"/> to the database
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveUserSettings();
        Task<bool> MarkRead(UserNotification notification);
    }
}