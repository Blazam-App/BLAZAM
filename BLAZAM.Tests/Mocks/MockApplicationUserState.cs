using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.Global.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BLAZAM.Tests.Mocks
{
    // --- Mock/Stub for IApplicationUserState (Helper for MFARequest tests) ---
    public class MockApplicationUserState : IApplicationUserState
    {
        // This is a minimal mock. Add properties or methods if MFARequest
        // interacts with IApplicationUserState in ways that need to be controlled during tests.
        // For the current MFARequest class, it's primarily just being stored.

        public int Id => throw new NotImplementedException();


        public string AuditUsername => throw new NotImplementedException();

        public string? Username { get; set; } = "test";

        public ClaimsPrincipal? Impersonator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsSuperAdmin => throw new NotImplementedException();

        public bool IsSelfEditOnly => throw new NotImplementedException();

        public DateTime LastAccessed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public AppUser Preferences => throw new NotImplementedException();

        public AuthenticationTicket? Ticket { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IApplicationUserSessionCache Cache { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? IPAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string LastUri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsAuthenticated => throw new NotImplementedException();

        public List<PermissionDelegate> PermissionDelegates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PermissionMapping> PermissionMappings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<NotificationSubscription> NotificationSubscriptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool HasUserPrivilege => throw new NotImplementedException();

        public bool HasCreateUserPrivilege => throw new NotImplementedException();

        public bool HasGroupPrivilege => throw new NotImplementedException();

        public bool HasCreateGroupPrivilege => throw new NotImplementedException();

        public bool HasOUPrivilege => throw new NotImplementedException();

        public bool HasCreateOUPrivilege => throw new NotImplementedException();

        public bool HasComputerPrivilege => throw new NotImplementedException();

        public bool HasBitLockerPrivilege => throw new NotImplementedException();

        public bool CanUnlockUsers => throw new NotImplementedException();

        public bool CanAssign => throw new NotImplementedException();

        public string DuoAuthState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IList<ReadNewsItem>? ReadNewsItems => throw new NotImplementedException();

        public bool ShowPluginPlaceholders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AppEvent OnSettingsChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AppEvent OnReadNewsSaved { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private string? _browser="Google Chrome";
        public string? Browser { get => _browser; set => _browser=value; }

        public bool CanSearchDisabled(ActiveDirectoryObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public void GetUserSettingFromDB()
        {
            throw new NotImplementedException();
        }

        public bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch)
        {
            throw new NotImplementedException();
        }

        public bool HasRole(string role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkAllRead()
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkRead(UserNotification notification)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveAllUserSettings()
        {
            throw new NotImplementedException();
        }

        public Task SaveBasicUserPreferences()
        {
            throw new NotImplementedException();
        }

        public Task SaveDashboardWidgets()
        {
            throw new NotImplementedException();
        }

        public Task SaveReadNewsItems()
        {
            throw new NotImplementedException();
        }
    }

}
