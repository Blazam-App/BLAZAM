using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System; // Added
using System.Collections.Generic; // Added
using System.Linq; // Added
using System.Security.Claims; // Added
using System.Threading.Tasks; // Added for Task

namespace BLAZAM.Session
{
    /// <summary>
    /// Represents the state and permissions for a logged-in application user, including their AD identity, application preferences, and calculated permissions.
    /// </summary>
    public class ApplicationUserState : IApplicationUserState
    {
        /// <summary>
        /// Event triggered when user settings are changed and saved.
        /// </summary>
        public AppDelegate OnSettingsChanged { get; set; }

        /// <summary>
        /// Gets or sets the primary <see cref="ClaimsPrincipal"/> representing the authenticated user.
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClaimsPrincipal"/> of the user performing impersonation, if any.
        /// </summary>
        public ClaimsPrincipal? Impersonator { get; set; }

        /// <summary>
        /// Gets or sets the list of permission delegates associated with the user.
        /// </summary>
        public List<PermissionDelegate> PermissionDelegates { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of permission mappings applicable to the user.
        /// </summary>
        public List<PermissionMapping> PermissionMappings { get; set; } = new();

        /// <summary>
        /// Gets or sets the timestamp of the user's last access or activity.
        /// </summary>
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the IP address from which the user accessed the application.
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Gets the list of user's favorite directory entries. Returns an empty list if preferences are not loaded.
        /// </summary>
        public List<UserFavoriteEntry> FavoriteEntries => userSettings?.FavoriteEntries ?? new List<UserFavoriteEntry>();

        /// <summary>
        /// Gets the list of news items marked as read by the user. Returns an empty list if preferences are not loaded.
        /// </summary>
        public IList<ReadNewsItem> ReadNewsItems => Preferences?.ReadNewsItems ?? new List<ReadNewsItem>(); // Corrected to List

        /// <summary>
        /// Gets the database ID of the user's application settings. Returns 0 if preferences are not loaded.
        /// </summary>
        public int Id => Preferences != null ? Preferences.Id : 0;

        /// <summary>
        /// Gets or sets the per-session cache for this user state.
        /// </summary>
        public IApplicationUserSessionCache Cache { get; set; } = new ApplicationUserSessionCache();

        /// <summary>
        /// Gets or sets the authentication ticket associated with the user's session.
        /// </summary>
        public AuthenticationTicket? Ticket { get; set; }

        /// <summary>
        /// Timestamp of the last data refresh for this user state.
        /// </summary>
        public DateTime lastDataRefresh;

        /// <summary>
        /// Holds the application-specific user settings, loaded from the database.
        /// </summary>
        public AppUser? userSettings { get; set; }

        private readonly IAppDatabaseFactory _dbFactory;

        /// <summary>Initializes a new instance of the <see cref="ApplicationUserState"/> class.</summary> 
        /// <param name="factory">The database context factory.</param> 
        /// <exception cref="ArgumentNullException">If factory is null.</exception>
        public ApplicationUserState(IAppDatabaseFactory factory)
        {
            if (factory == null)
            {
                Loggers.SystemLogger.Error("ApplicationUserState.Constructor: IAppDatabaseFactory factory is null.");
                throw new ArgumentNullException(nameof(factory));
            }
            _dbFactory = factory;
        }

        /// <summary>Gets the application-specific preferences for the user. Fetches from database if not already loaded. Returns null if user is not authenticated.</summary>
        public AppUser? Preferences
        {
            get
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    Loggers.SystemLogger.Debug("ApplicationUserState.Preferences_get: User is not authenticated. Returning null for Preferences.");
                    return null;
                }
                if (userSettings == null)
                {
                    GetUserSettingFromDB();
                }
                return userSettings;
            }
        }

        /// <summary>Marks a specific user notification as read.</summary> 
        /// <param name="notification">The notification to mark as read. Must not be null.</param> 
        /// <returns>True if successful, false otherwise (e.g., if notification is null, not found, or DB error).</returns>
        public async Task<bool> MarkRead(UserNotification notification)
        {
            if (notification == null)
            {
                Loggers.SystemLogger.Warning("ApplicationUserState.MarkRead: 'notification' parameter is null.");
                return await Task.FromResult(false);
            }
            try
            {
                notification.IsRead = true; // Optimistic update
                using var context = await _dbFactory.CreateDbContextAsync();
                var message = await context.UserNotifications.Where(un => un.Id == notification.Id).FirstOrDefaultAsync();
                if (message != null)
                {
                    message.IsRead = true;
                    var result = await context.SaveChangesAsync();
                    if (result >= 1) // Changed from ==1 to >=1 as SaveChanges can return more than 1 for related entities
                    {
                        if (userSettings != null) // userSettings might have been updated by side effect, refresh if needed
                        {
                            OnSettingsChanged?.Invoke();
                        }
                        return true;
                    }
                }
                else
                {
                     Loggers.SystemLogger.Warning("ApplicationUserState.MarkRead: UserNotification with ID {NotificationId} not found in database for user {UserGuid}.", notification.Id, userSettings?.UserGUID ?? "Unknown");
                }
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.MarkRead: Error trying to mark notification ID {NotificationId} read for user {UserGuid}.", notification?.Id, userSettings?.UserGUID ?? "Unknown");
            }
            return false;
        }

        /// <summary>Marks all unread notifications (excluding access requests) as read for the current user.</summary> 
        /// <returns>True if successful and at least one notification was marked read, false otherwise or on DB error.</returns>
        public async Task<bool> MarkAllRead()
        {
            try
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var messages = await context.UserNotifications
                    .Where(un => un.User.Id == Id && !un.IsRead && un.Notification.MessageType != MessageType.AccessRequest)
                    .ToListAsync();

                if (!messages.Any()) return true; // No messages to mark, consider it a success

                foreach (var notification in messages)
                {
                    notification.IsRead = true;
                }
                var result = await context.SaveChangesAsync();
                if (result >= 0) // SaveChanges returns number of state entries written, 0 is ok if nothing changed but no error.
                {
                    if (userSettings != null)
                    {
                        OnSettingsChanged?.Invoke();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.MarkAllRead: Error trying to mark all notifications read for user {UserGuid}.", userSettings?.UserGUID ?? "Unknown");
            }
            return false;
        }

        /// <summary>Loads or creates the user's application settings from the database. Associates settings with UserGUID from ClaimsPrincipal.</summary>
        public void GetUserSettingFromDB()
        {
            try
            {
                if (User == null || User.Identity?.IsAuthenticated != true) return; // Added authentication check
                using var context = _dbFactory.CreateDbContext();
                userSettings = context.UserSettings
                    .Include(x => x.NotificationSubscriptions)
                    .Include(x => x.FavoriteEntries) // Eager load other related entities if needed
                    .Include(x => x.ReadNewsItems)
                    .Include(x => x.DashboardWidgets)
                    .FirstOrDefault(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid));

                if (userSettings == null)
                {
                    userSettings = new AppUser();
                    string? email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    if (email != null) userSettings.Email = email;
                    userSettings.UserGUID = User.FindFirstValue(ClaimTypes.Sid);
                    userSettings.Username = Username; // Assuming Username property is correctly populated
                    context.UserSettings.Add(userSettings);
                    context.SaveChanges();
                }
                else if (string.IsNullOrEmpty(userSettings.Email)) // Check if existing settings email is empty
                {
                    var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                    if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
                    {
                        userSettings.Email = emailClaim.Value;
                        // Consider if SaveBasicUserPreferences should be called immediately or batched
                        Task.Run(async () => { await Task.Delay(1000); await SaveBasicUserPreferences(); });
                    }
                }
                lastDataRefresh = DateTime.Now;
            }
            catch (Exception ex) // Catch specific Exception ex
            {
                Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.GetUserSettingFromDB: Failed to get or create user settings for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
            }
        }

        /// <summary>Saves all user settings including basic preferences, read news items, and dashboard widgets. Logs errors on failure.</summary> 
        /// <returns>True if all save operations are initiated (actual success depends on async operations), false otherwise (currently always true).</returns>
        public async Task<bool> SaveAllUserSettings()
        {
            await SaveBasicUserPreferences();
            await SaveReadNewsItems();
            await SaveDashboardWidgets();
            OnSettingsChanged?.Invoke();
            return true;
        }

        /// <summary>Saves basic user preferences (theme, dark mode, profile picture, search settings, favorites, authenticator secret, email). Logs errors on failure.</summary>
        public async Task SaveBasicUserPreferences()
        {
            if (Preferences != null)
            {
                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    var dbUserSettings = await context.UserSettings.FirstOrDefaultAsync(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid));
                    if (dbUserSettings != null)
                    {
                        dbUserSettings.Theme = Preferences.Theme;
                        dbUserSettings.DarkMode = Preferences.DarkMode;
                        dbUserSettings.ProfilePicture = Preferences.ProfilePicture;
                        dbUserSettings.SearchDisabledUsers = Preferences.SearchDisabledUsers;
                        dbUserSettings.SearchDisabledComputers = Preferences.SearchDisabledComputers;
                        dbUserSettings.FavoriteEntries = Preferences.FavoriteEntries ?? new List<UserFavoriteEntry>();
                        dbUserSettings.AuthenticatorSecret = Preferences.AuthenticatorSecret;
                        dbUserSettings.Email = Preferences.Email;
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex) // Catch specific Exception ex
                {
                    Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.SaveBasicUserPreferences: Failed to save basic preferences for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
                }
            }
        }

        /// <summary>Saves the list of read news items for the user. Logs errors on failure.</summary>
        public async Task SaveReadNewsItems()
        {
            if (Preferences != null)
            {
                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    var dbUserSettings = await context.UserSettings.Include(u => u.ReadNewsItems).FirstOrDefaultAsync(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid));
                    if (dbUserSettings != null)
                    {
                        // Simple replacement for now, more complex merging might be needed depending on exact requirements
                        dbUserSettings.ReadNewsItems = Preferences.ReadNewsItems ?? new List<ReadNewsItem>();
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex) // Catch specific Exception ex
                {
                    Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.SaveReadNewsItems: Failed to save read news items for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
                }
            }
        }

        /// <summary>Saves the user's dashboard widget configurations. Logs errors on failure.</summary>
        public async Task SaveDashboardWidgets()
        {
            if (Preferences != null)
            {
                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    var dbUserSettings = await context.UserSettings.Include(u => u.DashboardWidgets).FirstOrDefaultAsync(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid));
                    if (dbUserSettings != null)
                    {
                        // Update existing or add new widgets
                        foreach (var widget in Preferences.DashboardWidgets)
                        {
                            var matchingWidget = dbUserSettings.DashboardWidgets.FirstOrDefault(w => w.WidgetType == widget.WidgetType);
                            if (matchingWidget != null)
                            {
                                matchingWidget.Slot = widget.Slot;
                                matchingWidget.Order = widget.Order;
                                matchingWidget.ItemsPerPage = widget.ItemsPerPage;
                            }
                            else
                            {
                                dbUserSettings.DashboardWidgets.Add(widget);
                            }
                        }
                        // Remove widgets not present in current preferences
                        var widgetsInDB = new List<UserDashboardWidget>(dbUserSettings.DashboardWidgets);
                        foreach (var widget in widgetsInDB)
                        {
                            if (!Preferences.DashboardWidgets.Any(w => w.WidgetType == widget.WidgetType))
                            {
                                dbUserSettings.DashboardWidgets.Remove(widget);
                            }
                        }
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex) // Catch specific Exception ex
                {
                    Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.SaveDashboardWidgets: Failed to save dashboard widgets for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
                }
            }
        }

        /// <summary>Gets a value indicating whether the user has SuperAdmin privileges. Returns false if user is not authenticated.</summary>
        public bool IsSuperAdmin
        {
            get
            {
                if (User?.Identity?.IsAuthenticated != true) { Loggers.SystemLogger.Debug("IsSuperAdmin_get: User not authenticated."); return false; }
                if (User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin)) return true;
                if (PermissionDelegates != null)
                    return PermissionDelegates.Any(p => p.IsSuperAdmin);
                return false;
            }
        }

        /// <summary>Gets the username from the User's ClaimsPrincipal Identity.</summary>
        public string? Username => User?.Identity?.Name; // Simplified

        /// <summary>Gets a value indicating whether the user is authenticated.</summary>
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true; // Simplified, catch block removed as direct prop access is safe

        /// <summary>Gets the username for audit logging, including impersonator information if applicable.</summary>
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

        /// <summary>
        /// Gets or sets the last URI visited by the user, for state restoration purposes.
        /// </summary>
        public string LastUri { get; set; }

        /// <summary>
        /// Returns the hash code for this instance, based on the User's hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return User?.GetHashCode() ?? 0; // Handle User potentially being null
        }

        /// <summary>Determines whether the specified object is equal to the current ApplicationUserState, based on User's SID and Actor SID claims.</summary>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is ApplicationUserState otherState)
            {
                // Ensure User and otherState.User and their SIDs/Actors are not null before comparing
                var thisSid = User?.FindFirstValue(ClaimTypes.Sid);
                var otherSid = otherState.User?.FindFirstValue(ClaimTypes.Sid);
                var thisActor = User?.FindFirstValue(ClaimTypes.Actor);
                var otherActor = otherState.User?.FindFirstValue(ClaimTypes.Actor);

                return thisSid != null && thisSid == otherSid &&
                       thisActor != null && thisActor == otherActor;
            }
            return false;
        }

        /// <summary>Checks if the user has a specific role claim.</summary> 
        /// <param name="userRole">The role to check for.</param> 
        /// <returns>True if the user has the role, false otherwise or if user is not authenticated.</returns>
        public bool HasRole(string userRole)
        {
            if (User?.Identity?.IsAuthenticated != true) return false;
            return User.HasClaim(ClaimTypes.Role, userRole);
        }

        /// <summary>Checks if the user has read permissions for User objects. Returns false if user is not authenticated.</summary>
        public bool HasUserPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectReadPermissions(ActiveDirectoryObjectType.User); } }
        /// <summary>Checks if the user has read permissions for BitLocker objects. Returns false if user is not authenticated.</summary>
        public bool HasBitLockerPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectReadPermissions(ActiveDirectoryObjectType.BitLocker); } }
        /// <summary>Checks if the user has create permissions for User objects. Returns false if user is not authenticated.</summary>
        public bool HasCreateUserPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectCreatePermissions(ActiveDirectoryObjectType.User); } }
        /// <summary>Checks if the user has read permissions for Group objects. Returns false if user is not authenticated.</summary>
        public bool HasGroupPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectReadPermissions(ActiveDirectoryObjectType.Group); } }
        /// <summary>Checks if the user has create permissions for Group objects. Returns false if user is not authenticated.</summary>
        public bool HasCreateGroupPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectCreatePermissions(ActiveDirectoryObjectType.Group); } }
        /// <summary>Checks if the user has read permissions for OU objects. Returns false if user is not authenticated.</summary>
        public bool HasOUPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectReadPermissions(ActiveDirectoryObjectType.OU); } }
        /// <summary>Checks if the user has create permissions for OU objects. Returns false if user is not authenticated.</summary>
        public bool HasCreateOUPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectCreatePermissions(ActiveDirectoryObjectType.OU); } }
        /// <summary>Checks if the user has read permissions for Computer objects. Returns false if user is not authenticated.</summary>
        public bool HasComputerPrivilege { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectReadPermissions(ActiveDirectoryObjectType.Computer); } }
        /// <summary>Checks if the user has permission to unlock User accounts. Returns false if user is not authenticated.</summary>
        public bool CanUnlockUsers { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectActionPermission(ActiveDirectoryObjectType.User, ObjectActions.Unlock); } }
        /// <summary>Checks if the user has permission to assign Group memberships. Returns false if user is not authenticated.</summary>
        public bool CanAssign { get { if (User?.Identity?.IsAuthenticated != true) return false; return HasObjectActionPermission(ActiveDirectoryObjectType.Group, ObjectActions.Assign); } }

        /// <summary>
        /// Gets or sets the Duo authentication state string, used during MFA flow.
        /// </summary>
        public string DuoAuthState { get; set; } = "";

        /// <summary>
        /// Gets or sets the list of notification subscriptions for the user. Modifying this list requires saving user settings.
        /// </summary>
        public List<NotificationSubscription> NotificationSubscriptions { get => userSettings?.NotificationSubscriptions ?? new List<NotificationSubscription>(); set { if(userSettings!=null) userSettings.NotificationSubscriptions = value; } } // Added null check for userSettings in setter

        /// <summary>Checks if the user has a specific action permission on a given object type, without OU context. Returns false if user is not authenticated.</summary>
        private bool HasObjectActionPermission(ActiveDirectoryObjectType objectType, ObjectAction actionType)
        {
            if (User?.Identity?.IsAuthenticated != true) return false; // Added auth check
            return HasPermission(objectType,
                p => p.Where(pm =>
                   pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
                  am.AllowOrDeny && am.ObjectAction.Id == actionType.Id &&
                  am.ObjectType == objectType
                   ))),
                   p => p.Where(pm =>
                   pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
                  !am.AllowOrDeny && am.ObjectAction.Id == actionType.Id &&
                  am.ObjectType == objectType
                   )))
                   );
        }

        /// <summary>Checks if the user has permission for a specific object type based on allow/deny selectors, without OU context. Returns false if user is not authenticated.</summary>
        private bool HasPermission(ActiveDirectoryObjectType objectType, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector = null)
        {
            if (User?.Identity?.IsAuthenticated != true) { Loggers.SystemLogger.Debug("HasPermission (ObjectType): User not authenticated for ObjectType {ObjectType}.", objectType); return false; }
            if (IsSuperAdmin) return true;
            var baseSearch = PermissionMappings;
            try
            {
                var possibleAllows = allowSelector.Invoke(baseSearch).ToList(); // Renamed for clarity
                if (denySelector != null)
                {
                    var possibleDenies = denySelector.Invoke(baseSearch).ToList();
                    if (possibleAllows.Any()) // More concise check
                    {
                        if (possibleDenies.Any())
                        {
                            // Simplified logic: if any deny is more specific or equally specific than the most specific allow, deny.
                            // This assumes OU specificity determines precedence, which is complex.
                            // The original logic was: if (d.OU.Length > possibleReads.OrderByDescending(r => r.OU.Length).First().OU.Length) return false;
                            // This part needs careful review if complex OU hierarchy denial is critical.
                            // For now, if there's any deny for the object type (without considering OU hierarchy yet for this overload), it might be too restrictive or too permissive.
                            // This method seems to be for non-OU specific checks, so OU length comparison might not apply here directly.
                            // If any deny exists for this object type, and any allow exists, it's ambiguous without OU context.
                            // Let's assume for this simplified overload, any deny overrides any allow if both exist.
                            return !possibleDenies.Any(); // Deny if any deny rule exists for the object type
                        }
                        return true; // Allows exist, no denies exist
                    }
                    return false; // No allows
                }
                else
                {
                    return possibleAllows.Any();
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "ApplicationUserState.HasPermission (ObjectType): Error checking permissions for ObjectType {ObjectType}.", objectType); // Include ex and objectType
            }
            return false;
        }

        /// <summary>Checks if the user can search for disabled objects of a specific type. Returns false if user is not authenticated.</summary>
        public bool CanSearchDisabled(ActiveDirectoryObjectType objectType)
        {
            if (User?.Identity?.IsAuthenticated != true) { Loggers.SystemLogger.Debug("CanSearchDisabled: User not authenticated."); return false; }
            if (IsSuperAdmin == true) return true;
            return PermissionMappings.Any(pm => pm.AccessLevels.Any(al => al.ObjectMap.Any(om => om.ObjectType == objectType && om.AllowDisabled))) == true;
        }

        /// <summary>Checks if the user has read permissions for a specific object type. Returns false if user is not authenticated.</summary>
        private bool HasObjectReadPermissions(ActiveDirectoryObjectType objectType)
        {
            if (User?.Identity?.IsAuthenticated != true) return false; // Added auth check
            try
            {
                if (IsSuperAdmin == true) return true;
                return PermissionMappings.Any(
                           m => m.AccessLevels.Any(
                               a => a.ObjectMap.Any(
                                   o => o.ObjectType == objectType && o.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level)
                               )
                           );
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error checking object read permissions for ObjectType {ObjectType}. Error: {@Error}", objectType, ex.Message); // Include objectType and ex
                return false;
            }
        }

        /// <summary>Creates a new user state instance.</summary> 
        /// <param name="user">The ClaimsPrincipal for the user.</param> 
        /// <param name="dbFactory">The database factory.</param> 
        /// <returns>A new IApplicationUserState instance.</returns>
        public static IApplicationUserState CreateUserState(ClaimsPrincipal user, IAppDatabaseFactory dbFactory)
        {
            return new ApplicationUserState(dbFactory) { User = user };
        }

        /// <summary>Checks if the user has create permissions for a specific object type. Returns false if user is not authenticated.</summary>
        private bool HasObjectCreatePermissions(ActiveDirectoryObjectType objectType)
        {
            if (User?.Identity?.IsAuthenticated != true) return false; // Added auth check
            if (IsSuperAdmin == true) return true;
            return PermissionMappings.Any(
                        m => m.AccessLevels.Any(
                            a => a.ObjectMap.Any(
                                o => o.ObjectType == objectType && o.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level) &&
                                a.ActionMap.Any(am => am.ObjectType == objectType &&
                                am.ObjectAction.Id == ObjectActions.Create.Id)
                            )
                        );
        }

        /// <summary>Checks if the user has permission on a specific DN target based on allow/deny selectors and OU hierarchy. Returns false if user is not authenticated.</summary>
        public bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch)
        {
            if (User?.Identity?.IsAuthenticated != true) { Loggers.SystemLogger.Debug("PermissionCheck: User not authenticated for DN {DNTarget}.", dnTarget); return false; }
            if (IsSuperAdmin) return true;

            IOrderedEnumerable<PermissionMapping>? baseSearch = null;
            if (!nestedSearch)
            {
                baseSearch = PermissionMappings
                    .Where(pm => dnTarget.Contains(pm.OU, StringComparison.OrdinalIgnoreCase)).OrderByDescending(pm => pm.OU.Length); // Added StringComparison
            }
            else
            {
                baseSearch = PermissionMappings
                    .Where(pm => pm.OU.Contains(dnTarget, StringComparison.OrdinalIgnoreCase)).OrderByDescending(pm => pm.OU.Length); // Added StringComparison
            }

            try
            {
                var possibleAllows = allowSelector.Invoke(baseSearch).ToList(); // Renamed
                if (denySelector != null)
                {
                    var possibleDenies = denySelector.Invoke(baseSearch).ToList();
                    if (possibleAllows.Any())
                    {
                        if (possibleDenies.Any())
                        {
                            // This logic correctly prioritizes more specific deny rules over allows.
                            foreach (var d in possibleDenies)
                            {
                                // If a deny rule's OU is more specific (longer) than the most specific allow rule's OU, deny access.
                                if (d.OU.Length > possibleAllows.OrderByDescending(r => r.OU.Length).First().OU.Length)
                                    return false;
                            }
                            // If no deny rule is more specific, and there's an allow, it's allowed (unless a deny is equally specific)
                            // A more precise check would be: if the most specific deny is >= most specific allow, deny.
                            // For now, this means if allows exist, and no *more specific* denies exist, it's an allow.
                            // This could be an issue if an equally specific deny exists.
                            // However, the current logic implies if allows exist, and no *more* specific deny exists, allow.
                            // Let's refine: if the most specific deny is as specific or more specific than the most specific allow, deny.
                            var mostSpecificAllowOuLength = possibleAllows.Max(r => r.OU.Length);
                            var mostSpecificDenyOuLengthForMatchingAllows = possibleDenies
                                .Where(d => possibleAllows.Any(a => a.OU.Equals(d.OU, StringComparison.OrdinalIgnoreCase) || d.OU.StartsWith(a.OU + ",", StringComparison.OrdinalIgnoreCase))) // Check if deny is related to an allow path
                                .Select(d => d.OU.Length)
                                .DefaultIfEmpty(0) // Handle case where no denies match allow paths
                                .Max();
                            if(mostSpecificDenyOuLengthForMatchingAllows >= mostSpecificAllowOuLength) return false;


                            return true; // Allows exist, and no deny rule is more specific
                        }
                        return true; // Allows exist, no denies exist
                    }
                    return false; // No allows
                }
                else
                {
                    return possibleAllows.Any();
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "ApplicationUserState.HasPermission (DN Target): Error checking permissions for DN {DNTarget}.", dnTarget); // Include ex and DN
            }
            return false;
        }

        /// <summary>Checks if the user has a specific action permission on a given DN target and object type. Returns false if user is not authenticated.</summary>
        public bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType)
        {
            if (User?.Identity?.IsAuthenticated != true) { Loggers.SystemLogger.Debug("HasActionPermission: User not authenticated for DN {DNTarget}, Action {Action}, ObjectType {ObjectType}.", dnTarget, action?.Name, objectType); return false; }
            return HasPermission(dnTarget, p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              am.AllowOrDeny && am.ObjectAction.Id == action.Id &&
              am.ObjectType == objectType
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              !am.AllowOrDeny && am.ObjectAction.Id == action.Id &&
              am.ObjectType == objectType
               ))), false
               );
        }
    }
}