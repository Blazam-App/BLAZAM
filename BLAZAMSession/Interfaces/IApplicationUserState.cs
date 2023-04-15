
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BLAZAM.Session.Interfaces
{
    public interface IApplicationUserState
    {
        string AuditUsername { get; }
        string Username { get; }
        ClaimsPrincipal? Impersonator { get; set; }
        bool IsSuperAdmin { get; }
        DateTime LastAccessed { get; set; }

        /// <summary>
        /// The web user who is currently logged in
        /// </summary>
        ClaimsPrincipal User { get; set; }
        AppUser? UserSettings { get; }
        AuthenticationTicket? Ticket { get; set; }
        IList<UserNotification> Messages { get; }
        IApplicationUserSessionCache Cache { get; set; }
        AppEvent<AppUser> OnSettingsChange { get; set; }
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
        bool CanUnlockUsers { get; set; }

        bool CanSearchDisabled(ActiveDirectoryObjectType objectType);
        bool Equals(object? obj);
        bool HasRole(string searchUsers);
        Task<bool> SaveUserSettings();
        Task<bool> MarkRead(UserNotification notification);
    }
}