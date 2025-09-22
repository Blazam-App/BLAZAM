using System.Security.Claims; // Added
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.Global.Data;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Session
{
    /// <summary>
    /// Represents the state and permissions for a logged-in application user, including their AD identity, application preferences, and calculated permissions.
    /// </summary>
    public class ApplicationUserState : IApplicationUserState, IDisposable
    {

        public AppEvent OnSettingsChanged { get; set; } = new();

        public AppEvent OnReadNewsSaved { get; set; } = new();


        public ClaimsPrincipal User { get; set; }


        public ClaimsPrincipal? Impersonator { get; set; }


        public List<PermissionDelegate> PermissionDelegates { get; set; } = new();


        public List<PermissionMapping> PermissionMappings { get; set; } = new();


        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

        public bool ShowPluginPlaceholders { get; set; } = false;

        public string? IPAddress { get; set; }

        /// <summary>
        /// Gets the list of user's favorite directory entries. Returns an empty list if preferences are not loaded.
        /// </summary>
        public List<UserFavoriteEntry> FavoriteEntries => userSettings?.FavoriteEntries ?? new List<UserFavoriteEntry>();

        public IList<ReadNewsItem> ReadNewsItems => Preferences?.ReadNewsItems ?? new List<ReadNewsItem>(); // Corrected to List


        public int Id => Preferences != null ? Preferences.Id : 0;


        public IApplicationUserSessionCache Cache { get; set; } = new ApplicationUserSessionCache();

        public AuthenticationTicket? Ticket { get; set; }


        /// <summary>
        /// Holds the application-specific user settings, loaded from the database.
        /// </summary>
        public AppUser? userSettings { get; set; }

        private readonly IAppDatabaseFactory _dbFactory;
        private bool _disposedValue;

        /// <summary>Initializes a new instance of the <see cref="ApplicationUserState"/> class.</summary> 
        /// <param name="factory">The database context factory.</param> 
        /// <exception cref="ArgumentNullException">If factory is null.</exception>
        public ApplicationUserState(IAppDatabaseFactory factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            _dbFactory = factory;
        }


        public AppUser? Preferences
        {
            get
            {

                if (userSettings == null)
                {
                    GetUserSettingFromDB();
                }
                return userSettings;
            }
        }


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


        public void GetUserSettingFromDB()
        {
            try
            {

                using var context = _dbFactory.CreateDbContext();
                userSettings = context.UserSettings
                    .Include(x => x.NotificationSubscriptions)
                    .Include(x => x.FavoriteEntries) // Eager load other related entities if needed
                    .Include(x => x.ReadNewsItems)
                    .Include(x => x.DashboardWidgets)
                    .FirstOrDefault(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid));

                if (userSettings == null)
                {
                    if (User.FindFirstValue(ClaimTypes.Sid) != null)
                    {
                        userSettings = new AppUser();
                        string? email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                        if (email != null) userSettings.Email = email;
                        userSettings.UserGUID = User.FindFirstValue(ClaimTypes.Sid);
                        userSettings.Username = Username; // Assuming Username property is correctly populated
                        context.UserSettings.Add(userSettings);
                        context.SaveChanges();
                    }
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
            }
            catch (Exception ex) // Catch specific Exception ex
            {
                Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.GetUserSettingFromDB: Failed to get or create user settings for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
            }
        }

        public async Task<bool> SaveAllUserSettings()
        {
            await SaveBasicUserPreferences();
            await SaveReadNewsItems();
            await SaveDashboardWidgets();
            OnSettingsChanged?.Invoke();
            return true;
        }

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
                        foreach (var newsItem in Preferences.ReadNewsItems)
                        {
                            if (newsItem.Id == 0)
                            {
                                dbUserSettings.ReadNewsItems.Add(newsItem);

                            }
                        }
                        // Simple replacement for now, more complex merging might be needed depending on exact requirements
                        await context.SaveChangesAsync();
                        OnReadNewsSaved.Invoke();
                    }
                }
                catch (Exception ex) // Catch specific Exception ex
                {
                    Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.SaveReadNewsItems: Failed to save read news items for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
                }
            }
        }

        public async Task SaveDashboardWidgets()
        {
            if (Preferences == null) return;

            try
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var dbUserSettings = await context.UserSettings
                    .Include(u => u.DashboardWidgets)
                    .FirstOrDefaultAsync(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid));

                if (dbUserSettings == null) return;

                // Build lookup dictionaries for efficient access
                var prefWidgets = Preferences.DashboardWidgets.ToDictionary(w => w.WidgetType);
                var dbWidgets = dbUserSettings.DashboardWidgets.ToDictionary(w => w.WidgetType);

                // Update existing widgets and add new ones
                foreach (var widgetType in prefWidgets.Keys)
                {
                    if (dbWidgets.TryGetValue(widgetType, out var dbWidget))
                    {
                        var prefWidget = prefWidgets[widgetType];
                        dbWidget.Slot = prefWidget.Slot;
                        dbWidget.Order = prefWidget.Order;
                        dbWidget.ItemsPerPage = prefWidget.ItemsPerPage;
                    }
                    else
                    {
                        dbUserSettings.DashboardWidgets.Add(prefWidgets[widgetType]);
                    }
                }

                // Remove widgets not present in preferences
                var widgetsToRemove = dbWidgets.Keys.Except(prefWidgets.Keys).ToList();
                foreach (var widgetType in widgetsToRemove)
                {
                    var widget = dbWidgets[widgetType];
                    dbUserSettings.DashboardWidgets.Remove(widget);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error(ex, "ApplicationUserState.SaveDashboardWidgets: Failed to save dashboard widgets for UserGUID {UserGuid}.", User?.FindFirstValue(ClaimTypes.Sid) ?? "Unknown");
            }
        }

        public bool IsSuperAdmin
        {
            get
            {
                if (User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin)) return true;
                if (PermissionDelegates != null)
                    return PermissionDelegates.Any(p => p.IsSuperAdmin);
                return false;
            }
        }

        public string? Username => User?.Identity?.Name;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

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
            return User?.GetHashCode() ?? 0;
        }


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


        public bool HasRole(string role)
        {

            return User.HasClaim(ClaimTypes.Role, role);
        }

        public bool HasUserPrivilege => HasObjectReadPermissions(ActiveDirectoryObjectType.User);

        public bool HasBitLockerPrivilege => HasObjectReadPermissions(ActiveDirectoryObjectType.BitLocker);

        public bool HasCreateUserPrivilege => HasObjectCreatePermissions(ActiveDirectoryObjectType.User);

        public bool HasGroupPrivilege => HasObjectReadPermissions(ActiveDirectoryObjectType.Group);

        public bool HasCreateGroupPrivilege => HasObjectCreatePermissions(ActiveDirectoryObjectType.Group);

        public bool HasOUPrivilege => HasObjectReadPermissions(ActiveDirectoryObjectType.OU);

        public bool HasCreateOUPrivilege => HasObjectCreatePermissions(ActiveDirectoryObjectType.OU);

        public bool HasComputerPrivilege => HasObjectReadPermissions(ActiveDirectoryObjectType.Computer);

        public bool CanUnlockUsers => HasObjectActionPermission(ActiveDirectoryObjectType.User, ObjectActions.Unlock);

        public bool CanAssign => HasObjectActionPermission(ActiveDirectoryObjectType.Group, ObjectActions.Assign);

        /// <summary>
        /// Gets or sets the Duo authentication state string, used during MFA flow.
        /// </summary>
        public string DuoAuthState { get; set; } = "";

        /// <summary>
        /// Gets or sets the list of notification subscriptions for the user. Modifying this list requires saving user settings.
        /// </summary>
        public List<NotificationSubscription> NotificationSubscriptions { get => userSettings?.NotificationSubscriptions ?? new List<NotificationSubscription>(); set { if (userSettings != null) userSettings.NotificationSubscriptions = value; } } // Added null check for userSettings in setter

        /// <summary>
        /// Checks if the user has a specific action permission on a given object type, without OU context.
        /// </summary>
        private bool HasObjectActionPermission(ActiveDirectoryObjectType objectType, ObjectAction actionType)
        {

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

        /// <summary>
        /// Checks if the user has permission for a specific object type based on allow/deny selectors, without OU context.
        /// </summary>
        private bool HasPermission(ActiveDirectoryObjectType objectType, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector = null)
        {
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
                        return !possibleDenies.Any();
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


        public bool CanSearchDisabled(ActiveDirectoryObjectType objectType)
        {
            if (IsSuperAdmin == true) return true;
            return PermissionMappings.Any(pm => pm.AccessLevels.Any(al => al.ObjectMap.Any(om => om.ObjectType == objectType && om.AllowDisabled))) == true;
        }

        /// <summary>Checks if the user has read permissions for a specific object type.
        private bool HasObjectReadPermissions(ActiveDirectoryObjectType objectType)
        {

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
                Loggers.SystemLogger.Error(ex, "Error checking object read permissions for ObjectType {ObjectType}", objectType); // Include objectType and ex
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

        /// <summary>Checks if the user has create permissions for a specific object type.</summary>
        private bool HasObjectCreatePermissions(ActiveDirectoryObjectType objectType)
        {
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

        private bool CheckDenyPermissions(List<PermissionMapping> possibleAllows, List<PermissionMapping> possibleDenies)
        {
            if (!possibleAllows.Any()) return false; // No allows, so deny
            if (!possibleDenies.Any()) return true; // No denies, so allow

            foreach (var d in possibleDenies)
            {
                if (d.OU.Length > possibleAllows.OrderByDescending(r => r.OU.Length).First().OU.Length)
                    return false;
            }

            var mostSpecificAllowOuLength = possibleAllows.Max(r => r.OU.Length);
            var mostSpecificDenyOuLengthForMatchingAllows = possibleDenies
                .Where(d => possibleAllows.Any(a => a.OU.Equals(d.OU, StringComparison.OrdinalIgnoreCase) || d.OU.StartsWith(a.OU + ",", StringComparison.OrdinalIgnoreCase)))
                .Select(d => d.OU.Length)
                .DefaultIfEmpty(0)
                .Max();

            if (mostSpecificDenyOuLengthForMatchingAllows >= mostSpecificAllowOuLength) return false;

            return true;
        }
        public bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch)
        {
            if (IsSuperAdmin) return true;

            IOrderedEnumerable<PermissionMapping>? baseSearch = null;
            if (!nestedSearch)
            {
                baseSearch = PermissionMappings
                    .Where(pm => dnTarget.Contains(pm.OU, StringComparison.OrdinalIgnoreCase)).OrderByDescending(pm => pm.OU.Length);
            }
            else
            {
                baseSearch = PermissionMappings
                    .Where(pm => pm.OU.Contains(dnTarget, StringComparison.OrdinalIgnoreCase)).OrderByDescending(pm => pm.OU.Length);
            }

            try
            {
                var possibleAllows = allowSelector.Invoke(baseSearch).ToList();
                if (!possibleAllows.Any()) return false;

                if (denySelector != null)
                {
                    var possibleDenies = denySelector.Invoke(baseSearch).ToList();
                    return CheckDenyPermissions(possibleAllows, possibleDenies);
                }
                else
                {
                    return true; // Has allows, no deny selector
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "ApplicationUserState.HasPermission (DN Target): Error checking permissions for DN {DNTarget}.", dnTarget);
            }
            return false;
        }

        public bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType)
        {
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    Cache?.Dispose();


                    // Unsubscribe event handlers if needed
                    OnSettingsChanged = null;

                    // Set large fields to null
                    PermissionDelegates = null;
                    PermissionMappings = null;
                    userSettings = null;
                    User = null;
                    Impersonator = null;
                    Ticket = null;
                }

                // Free unmanaged resources (none in this class)
                _disposedValue = true;
            }
        }


        //~ApplicationUserState()
        //{
        //    Dispose(disposing: false);
        //}

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}