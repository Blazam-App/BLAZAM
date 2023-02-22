using Microsoft.EntityFrameworkCore;
using System.DirectoryServices;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.Database;
using BLAZAM.Server.Data.Services;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{

    public interface IActiveDirectory
    {
        IApplicationUserStateService? UserStateService { get; }
        DatabaseContext? Context { get; }
        IDbContextFactory<DatabaseContext>? Factory { get; }
        bool Pingable { get; }
        bool PortOpen { get; }
        DirectoryConnectionStatus Status { get; }
        DirectoryEntry? AppRootDirectoryEntry { get; }
        IADOUSearcher? OUs { get; }
        IADGroupSearcher? Groups { get; }
        IADUserSearcher? Users { get; }
        AppEvent<DirectoryConnectionStatus>? OnStatusChanged { get; set; }
        ADSettings? ConnectionSettings { get; }
        AppEvent<IApplicationUserState>? OnNewLoginUser { get; set; }
        IADComputerSearcher? Computers { get; }
        IEncryptionService Encryption { get; }

        /// <summary>
        /// Authenticates a login request's credentials against the configured
        /// Activer Directory connection in the application settings database.
        /// </summary>
        /// <param name="loginReq">The login request to validate</param>
        /// <returns>The Active Directory user who authenticated, or null if the credentials were invalid, or connection
        /// attempt failed.</returns>
        IADUser? Authenticate(LoginRequest loginReq);
        void Connect();

        /// <summary>
        /// connects to an Active Directory server.
        /// It first checks the database connection status, then retrieves the Active 
        /// Directory settings from the database.Sets the Status property to reflect the
        /// server connectivity.
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();
        void Dispose();
        DirectoryEntry GetDeleteObjectsEntry();
        DirectoryEntry GetDirectoryEntry(string? baseDN = null);
        bool RestoreTombstone(IDirectoryModel model, IADOrganizationalUnit newOU);
    }
}