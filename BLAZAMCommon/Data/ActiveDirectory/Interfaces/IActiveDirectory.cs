using Microsoft.EntityFrameworkCore;
using System.DirectoryServices;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Data.Database;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Provides a connection to an Active Directory Domain
    /// </summary>
    public interface IActiveDirectoryContext
    {
        IApplicationUserStateService UserStateService { get; }
        IDatabaseContext? Context { get; }
        AppDatabaseFactory Factory { get; }

        /// <summary>
        /// Checks whether the configured Active Directory port is open for connections
        /// </summary>
        bool PortOpen { get; }

        /// <summary>
        /// The current status of the Active Directory connection
        /// </summary>
        DirectoryConnectionStatus Status { get; }

        /// <summary>
        /// The application scoped directory entry root
        /// </summary>
        DirectoryEntry? AppRootDirectoryEntry { get; }

        /// <summary>
        /// Provides OU search functions
        /// </summary>
        IADOUSearcher OUs { get; }

        /// <summary>
        /// Provides Group search functions
        /// </summary>
        IADGroupSearcher Groups { get; }

        /// <summary>
        /// Provides User search functions
        /// </summary>
        IADUserSearcher Users { get; }

        /// <summary>
        /// Provides Computer search functions
        /// </summary>
        IADComputerSearcher Computers { get; }

        /// <summary>
        /// Called when the connection state of the Active Directory server has
        /// changed
        /// </summary>
        AppEvent<DirectoryConnectionStatus>? OnStatusChanged { get; set; }
        
        ADSettings? ConnectionSettings { get; }

        /// <summary>
        /// Called when a new user login matches an Active Directory user
        /// </summary>
        AppEvent<IApplicationUserState>? OnNewLoginUser { get; set; }
        IApplicationUserState? CurrentUser { get; }

        IDirectoryEntryAdapter? GetDirectoryModelBySid(string sid);
        
        IDirectoryEntryAdapter? FindEntryBySID(byte[] sid);


        
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
        /// <summary>
        /// Retuns the directory entry of the given Base distiguished
        /// name. If no base is provided,  the application scope BaseDN setting
        /// will be used
        /// </summary>
        /// <param name="baseDN"></param>
        /// <returns></returns>
        DirectoryEntry GetDirectoryEntry(string? baseDN = null);
        bool RestoreTombstone(IDirectoryEntryAdapter model, IADOrganizationalUnit newOU);
    }
}