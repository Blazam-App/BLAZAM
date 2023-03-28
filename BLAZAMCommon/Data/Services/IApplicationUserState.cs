using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Models.Database.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BLAZAM.Common.Data.Services
{
    public interface IApplicationUserState
    {
        string AuditUsername { get; }
        string Username { get; }
        IADUser? DirectoryUser { get; set; }
        ClaimsPrincipal? Impersonator { get; set; }
        bool IsSuperAdmin { get; }
        DateTime LastAccessed { get; set; }
        ClaimsPrincipal User { get; set; }
        AppUser? UserSettings { get; }
        AuthenticationTicket Ticket { get; set; }
        IList<NotificationMessage> Messages { get; set; }
        IApplicationUserSessionCache Cache { get; set; }
        AppEvent<AppUser> OnSettingsChange { get; set; }

        bool Equals(object? obj);
        bool HasRole(string searchUsers);
        Task<bool> SaveUserSettings();
    }
}