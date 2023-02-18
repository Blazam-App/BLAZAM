using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Models.Database.User;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BLAZAM.Common.Data.Services
{
    public interface IApplicationUserState
    {
        string AuditUsername { get; }
        IDbContextFactory<DatabaseContext> DbFactory { get; set; }
        IADUser? DirectoryUser { get; set; }
        ClaimsPrincipal? Impersonator { get; set; }
        bool IsSuperAdmin { get; }
        DateTime LastAccessed { get; set; }
        ClaimsPrincipal User { get; set; }
        UserSettings? UserSettings { get; }
        bool Equals(object? obj);
        bool HasRole(string searchUsers);
        Task<bool> SaveUserSettings();
    }
}