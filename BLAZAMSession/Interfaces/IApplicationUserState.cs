using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using Microsoft.AspNetCore.Authentication;
using System; // Required for Func, DateTime
using System.Collections.Generic; // Required for List, IList
using System.Security.Claims; // Required for ClaimsPrincipal
using System.Threading.Tasks; // Required for Task

namespace BLAZAM.Session.Interfaces
{
    /// <summary>
    /// Defines the contract for representing a user's application state, including identity, permissions, and preferences.
    /// </summary>
    public interface IApplicationUserState
    {
        /// <summary>
        /// Gets this user's unique ID as stored in the application database (corresponds to AppUser.Id).
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Event triggered when user-specific settings or preferences change.
        /// </summary>
        AppDelegate OnSettingsChanged { get; set; }

        /// <summary>
        /// Gets the username for audit logging, including impersonator information if applicable (e.g., "UserA impersonated by AdminUser").
        /// </summary>
        string AuditUsername { get; }

        /// <summary>
        /// Gets the username from the User's ClaimsPrincipal Identity.
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// Gets or sets the <see cref="System.Security.Claims.ClaimsPrincipal"/> of the user who is impersonating the current <see cref="User"/>, if applicable.
        /// </summary>
        ClaimsPrincipal? Impersonator { get; set; }

        /// <summary>
        /// Gets a value indicating whether the user has SuperAdmin privileges.
        /// </summary>
        bool IsSuperAdmin { get; }

        /// <summary>
        /// Gets or sets the timestamp of the user's last access or activity.
        /// </summary>
        DateTime LastAccessed { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Security.Claims.ClaimsPrincipal"/> representing the user's identity.
        /// </summary>
        ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Gets the application-specific preferences (e.g., theme, dark mode) for the user. See <see cref="BLAZAM.Database.Models.User.AppUser"/>.
        /// </summary>
        /// <remarks>
        /// Changes made to the returned object are not saved until a relevant Save method (e.g., <see cref="SaveAllUserSettings()"/>) is called.
        /// </remarks>
        AppUser Preferences { get; }

        /// <summary>
        /// Gets or sets the authentication ticket associated with the user's session. This may be used for cookie management.
        /// </summary>
        AuthenticationTicket? Ticket { get; set; }

        /// <summary>
        /// Gets or sets the per-user session cache instance.
        /// </summary>
        IApplicationUserSessionCache Cache { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the user accessed the application.
        /// </summary>
        string? IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the last URI visited by the user, for state restoration purposes.
        /// </summary>
        string LastUri { get; set; }

        /// <summary>
        /// Gets a value indicating whether the user is authenticated based on their <see cref="User"/> ClaimsPrincipal.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets or sets the list of <see cref="BLAZAM.Database.Models.Permissions.PermissionDelegate"/> objects assigned to this user.
        /// </summary>
        List<PermissionDelegate> PermissionDelegates { get; set; }

        /// <summary>
        /// Gets or sets the list of effective <see cref="BLAZAM.Database.Models.Permissions.PermissionMapping"/> objects for this user, derived from their delegates and OU structure.
        /// </summary>
        List<PermissionMapping> PermissionMappings { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="BLAZAM.Database.Models.Notifications.NotificationSubscription"/> for the user.
        /// </summary>
        List<NotificationSubscription> NotificationSubscriptions { get; set; }

        /// <summary>Gets a value indicating whether the user has read permissions for User objects.</summary>
        bool HasUserPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has create permissions for User objects.</summary>
        bool HasCreateUserPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has read permissions for Group objects.</summary>
        bool HasGroupPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has create permissions for Group objects.</summary>
        bool HasCreateGroupPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has read permissions for OU objects.</summary>
        bool HasOUPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has create permissions for OU objects.</summary>
        bool HasCreateOUPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has read permissions for Computer objects.</summary>
        bool HasComputerPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has read permissions for BitLocker objects.</summary>
        bool HasBitLockerPrivilege { get; }
        /// <summary>Gets a value indicating whether the user has permission to unlock User accounts.</summary>
        bool CanUnlockUsers { get; }
        /// <summary>Gets a value indicating whether the user has permission to assign Group memberships.</summary>
        bool CanAssign { get; }

        /// <summary>
        /// Gets or sets the Duo authentication state string, used during MFA flow.
        /// </summary>
        string DuoAuthState { get; set; }

        /// <summary>
        /// Gets the list of news items marked as read by the user.
        /// </summary>
        IList<ReadNewsItem>? ReadNewsItems { get; }

        /// <summary>
        /// Checks if the user can search for disabled objects of a specific type based on their permissions.
        /// </summary>
        /// <param name="objectType">The type of Active Directory object.</param>
        /// <returns>True if the user can search for disabled objects of this type; otherwise, false.</returns>
        bool CanSearchDisabled(ActiveDirectoryObjectType objectType);

        /// <summary>
        /// Checks if the user has a specific role claim.
        /// </summary>
        /// <param name="role">The role to check for.</param>
        /// <returns>True if the user has the role claim; otherwise, false.</returns>
        bool HasRole(string role);

        /// <summary>
        /// Saves all modifiable user settings (preferences, read news items, dashboard widgets) to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation, returning true if successful.</returns>
        Task<bool> SaveAllUserSettings();

        /// <summary>
        /// Marks a specific user notification as read.
        /// </summary>
        /// <param name="notification">The notification to mark as read.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if successful.</returns>
        Task<bool> MarkRead(UserNotification notification);

        /// <summary>
        /// Determines if the user has a specific permission based on allow/deny selectors applied to their permission mappings for a given DN target.
        /// </summary>
        /// <param name="dnTarget">The distinguished name of the target object or OU.</param>
        /// <param name="allowSelector">A function to filter permission mappings for allowed access.</param>
        /// <param name="denySelector">An optional function to filter permission mappings for denied access.</param>
        /// <param name="nestedSearch">True to search for permissions on OUs containing the dnTarget (e.g., parent OU permissions apply); false to search for permissions on OUs contained by dnTarget (e.g., child OU permissions).</param>
        /// <returns>True if the user has the permission; otherwise, false.</returns>
        bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch);

        /// <summary>
        /// Determines if the user has permission to perform a specific action on a given object type at a specific DN target.
        /// </summary>
        /// <param name="dnTarget">The distinguished name of the target object or OU.</param>
        /// <param name="action">The object action to check.</param>
        /// <param name="objectType">The type of Active Directory object.</param>
        /// <returns>True if the user has the action permission; otherwise, false.</returns>
        bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType);

        /// <summary>
        /// Saves the user's dashboard widget configurations to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveDashboardWidgets();

        /// <summary>
        /// Saves the list of read news items for the user to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveReadNewsItems();

        /// <summary>
        /// Saves basic user preferences (e.g., theme, dark mode, email) to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveBasicUserPreferences();

        /// <summary>
        /// Loads or reloads the user's application settings from the database.
        /// </summary>
        void GetUserSettingFromDB();

        /// <summary>
        /// Marks all unread notifications (excluding access requests) as read for the current user.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, returning true if successful.</returns>
        Task<bool> MarkAllRead();
    }
}