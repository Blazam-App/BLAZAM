using Microsoft.EntityFrameworkCore;
using System.DirectoryServices;
using BLAZAM.Session.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Common.Data;
using System.DirectoryServices.ActiveDirectory;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Provides a connection to an Active Directory Domain
    /// </summary>
    public interface IActiveDirectoryContext
    {
        IApplicationUserStateService UserStateService { get; }
        IDatabaseContext? Context { get; }
        IAppDatabaseFactory Factory { get; }

        /// <summary>
        /// Checks whether the configured Active Directory port is open for connections
        /// </summary>
        bool PortOpen { get; }

        /// <summary>
        /// The current status of the Active Directory connection
        /// </summary>
        DirectoryConnectionStatus Status { get; }

        /// <summary>
        /// How many time the connection has failed.
        /// Max value should not exceed 10
        /// </summary>
        int FailedConnectionAttempts { get; set; }

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
        /// Provides Printer search functions
        /// </summary>
        IADPrinterSearcher Printers { get; }

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

        /// <summary>
        /// Provides an impersonation context to the application domain account.
        /// </summary>
        WindowsImpersonation Impersonation { get; }
        List<DomainController> DomainControllers { get; }

        IDirectoryEntryAdapter? GetDirectoryEntryBySid(string sid);

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