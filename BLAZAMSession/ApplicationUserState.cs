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

        public AppEvent OnSettingsChanged { get; set; }

        public ClaimsPrincipal User { get; set; }

        public ClaimsPrincipal? Impersonator { get; set; }

        public List<PermissionDelegate> PermissionDelegates { get; set; } = new();


        public List<PermissionMapping> PermissionMappings { get; set; } = new();


        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;


        public string? IPAddress { get; set; }

        public List<UserFavoriteEntry> FavoriteEntries => userSettings?.FavoriteEntries ?? new List<UserFavoriteEntry>();

        //public IList<UserNotification>? Notifications
        //{
        //    get
        //    {
        //        if (User.Identity?.IsAuthenticated != true) return default;
        //        if ((DateTime.Now - lastDataRefresh).TotalSeconds > 1)
        //            GetUserSettingFromDB();
        //        return userSettings?.Notifications.Where(m => !m.IsRead).ToList();

        //    }
        //}
        public IList<ReadNewsItem> ReadNewsItems => Preferences?.ReadNewsItems ?? new();

        public int Id => Preferences != null ? Preferences.Id : 0;


        public IApplicationUserSessionCache Cache { get; set; } = new ApplicationUserSessionCache();

        public AuthenticationTicket? Ticket { get; set; }

        public DateTime lastDataRefresh;
        public AppUser? userSettings { get; set; }

        private readonly IAppDatabaseFactory _dbFactory;

        public ApplicationUserState(IAppDatabaseFactory factory)
        {

            _dbFactory = factory;


        }



        public AppUser? Preferences
        {
            get
            {
                if (User.Identity?.IsAuthenticated != true) return null;
                if (userSettings == null)
                {

                    GetUserSettingFromDB();
                }
                return userSettings;
            }
        }


        public async Task<bool> MarkRead(UserNotification notification)
        {
            notification.IsRead = true;
            using var context = await _dbFactory.CreateDbContextAsync();
            var message = context.UserNotifications.Where(un => un.Id == notification.Id).FirstOrDefault(); ;
            if (message != null)
            {
                message.IsRead = true;
                var result = await context.SaveChangesAsync();

                if (result == 1)
                {
                    //GetUserSettingFromDB();
                    if (userSettings != null)
                    {
                        OnSettingsChanged?.Invoke();
                    }

                    return true;
                }
            }
            return false;
        }

        private void GetUserSettingFromDB()
        {
            try
            {
                if (User == null) return;
                using var context = _dbFactory.CreateDbContext();

                userSettings = context.UserSettings.Include(x => x.NotificationSubscriptions).Where(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid)).FirstOrDefault();
                if (userSettings == null)
                {
                    userSettings = new AppUser();
                    string? email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    if (email != null)
                    {
                        userSettings.Email = email;
                    }
                    userSettings.UserGUID = User.FindFirstValue(ClaimTypes.Sid);
                    userSettings.Username = Username;
                    context.UserSettings.Add(userSettings);

                    context.SaveChanges();

                }
                else if (Preferences != null && Preferences.Email == null)
                {
                    var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                    if (emailClaim != null && !emailClaim.Value.IsNullOrEmpty())
                    {

                        Preferences.Email = emailClaim.Value;
                        Task.Run(() =>
                        {
                            Task.Delay(1000).Wait();
                            SaveBasicUserPreferences();

                        });
                    }
                }

                lastDataRefresh = DateTime.Now;
            }
            catch
            {

            }
        }


        public async Task<bool> SaveAllUserSettings()
        {


            await SaveBasicUserPreferences();

            await SaveReadNewsItems();
            await SaveDashboardWidgets();
            //GetUserSettingFromDB();
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
                    var dbUserSettings = await context.UserSettings.Where(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid)).FirstOrDefaultAsync();
                    if (dbUserSettings != null)
                    {
                        dbUserSettings.Theme = this.Preferences?.Theme;
                        dbUserSettings.DarkMode = this.Preferences?.DarkMode == true;
                        dbUserSettings.ProfilePicture = this.Preferences?.ProfilePicture;
                        dbUserSettings.SearchDisabledUsers = this.Preferences?.SearchDisabledUsers == true;
                        dbUserSettings.SearchDisabledComputers = this.Preferences?.SearchDisabledComputers == true;
                        dbUserSettings.FavoriteEntries = this.Preferences?.FavoriteEntries ?? new();
                        dbUserSettings.Email = this.Preferences?.Email;
                        await context.SaveChangesAsync();

                    }

                }
                catch { }

            }
        }
        public async Task SaveReadNewsItems()
        {
            if (Preferences != null)
            {
                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    var dbUserSettings = await context.UserSettings.Where(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid)).FirstOrDefaultAsync();
                    if (dbUserSettings != null)
                    {
                        dbUserSettings.ReadNewsItems = this.Preferences?.ReadNewsItems ?? new();

                        await context.SaveChangesAsync();

                    }

                }
                catch { }

            }
        }

        public async Task SaveDashboardWidgets()
        {
            if (Preferences != null)
            {
                try
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    var dbUserSettings = await context.UserSettings.Where(us => us.UserGUID == User.FindFirstValue(ClaimTypes.Sid)).FirstOrDefaultAsync();
                    if (dbUserSettings != null)
                    {
                        foreach (var widget in Preferences.DashboardWidgets)
                        {
                            var matchingWidget = dbUserSettings?.DashboardWidgets.FirstOrDefault(w => w.WidgetType == widget.WidgetType);
                            if (matchingWidget != null)
                            {
                                matchingWidget.Slot = widget.Slot;
                                matchingWidget.Order = widget.Order;
                                matchingWidget.ItemsPerPage = widget.ItemsPerPage;
                            }
                            else
                            {
                                dbUserSettings?.DashboardWidgets.Add(widget);
                            }
                        }
                        var widgetsInDB = new List<UserDashboardWidget>(dbUserSettings?.DashboardWidgets);
                        foreach (var widget in widgetsInDB)
                        {
                            if (!this.Preferences.DashboardWidgets.Any(w => w.WidgetType == widget.WidgetType))
                            {
                                dbUserSettings.DashboardWidgets.Remove(widget);
                            }

                        }
                        await context.SaveChangesAsync();

                    }

                }
                catch { }

            }
        }

        public bool IsSuperAdmin
        {
            get
            {
                if (User == null) return false;
                if (User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin)) return true;
                if (PermissionDelegates != null)
                    return PermissionDelegates.Any(p => p.IsSuperAdmin);
                return false;
            }
        }

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
                    return User.Identity?.IsAuthenticated == true;
                }
                catch
                {
                    return false;
                }
            }
        }


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

        public string DuoAuthState { get; set; } = "";
        public List<NotificationSubscription> NotificationSubscriptions { get => userSettings?.NotificationSubscriptions; set { userSettings.NotificationSubscriptions = value; } }

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
        /// Used to check application user permissions. The selectors provided will authomatically search for this DirectoryModel's
        /// OU. Supplied selectors should only check for AccessLevels.Any(...) that match the permission type requested.
        /// </summary>
        /// <param name="allowSelector"></param>
        /// <param name="denySelector"></param>
        /// <returns></returns>
        private bool HasPermission(ActiveDirectoryObjectType objectType, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector = null)
        {

            if (IsSuperAdmin) return true;

            var baseSearch = PermissionMappings;


            try
            {
                var possibleReads = allowSelector.Invoke(baseSearch).ToList();
                if (denySelector != null)
                {
                    var possibleDenys = denySelector.Invoke(baseSearch).ToList();

                    if (possibleReads != null && possibleReads.Count > 0)
                    {
                        if (possibleDenys != null && possibleDenys.Count > 0)
                        {
                            foreach (var d in possibleDenys)
                            {
                                if (d.OU.Length > possibleReads.OrderByDescending(r => r.OU.Length).First().OU.Length)
                                    return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return possibleReads?.Count > 0;
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message);
            }
            return false;
        }
        /// <summary>
        /// Checks that the user has some kind of read access for disabled objects of
        /// this type in any place on the OU tree.
        /// </summary>
        public bool CanSearchDisabled(ActiveDirectoryObjectType objectType)
        {

            if (IsSuperAdmin == true) return true;

            return PermissionMappings.Any(pm => pm.AccessLevels.Any(al => al.ObjectMap.Any(om => om.ObjectType == objectType && om.AllowDisabled))) == true;


        }


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
                Loggers.SystemLogger.Error("Error checking object read permissions {@Error}", ex);
                return false;
            }
        }
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

        public bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch)
        {
            if (IsSuperAdmin) return true;

            IOrderedEnumerable<PermissionMapping>? baseSearch = null;
            if (!nestedSearch)
            {
                baseSearch = PermissionMappings
       .Where(pm => dnTarget.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length);

            }
            else
            {
                baseSearch = PermissionMappings
       .Where(pm => pm.OU.Contains(dnTarget)).OrderByDescending(pm => pm.OU.Length);

            }

            if (baseSearch == null)
            {
                Loggers.ActiveDirectoryLogger.Error("The active user state for " + dnTarget + " could not" +
                    "be found in the application cache." + " {@Error}", new ApplicationException());
                return false;
            }
            try
            {
                var possibleReads = allowSelector.Invoke(baseSearch).ToList();
                if (denySelector != null)
                {
                    var possibleDenys = denySelector.Invoke(baseSearch).ToList();

                    if (possibleReads != null && possibleReads.Count > 0)
                    {
                        if (possibleDenys != null && possibleDenys.Count > 0)
                        {
                            foreach (var d in possibleDenys)
                            {
                                if (d.OU.Length > possibleReads.OrderByDescending(r => r.OU.Length).First().OU.Length)
                                    return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return possibleReads?.Count > 0;
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message);
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
    }
}