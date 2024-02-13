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
    public interface IActiveDirectoryContext:IDisposable
    {
        /// <summary>
        /// The database factory to use for this connection
        /// </summary>
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

        /// <summary>
        /// The connection settings as gotten from the <see cref="Factory"/>
        /// </summary>
        ADSettings? ConnectionSettings { get; }

        /// <summary>
        /// Called when a new user login matches an Active Directory user
        /// </summary>
        AppEvent<IApplicationUserState>? OnNewLoginUser { get; set; }

        /// <summary>
        /// The current web user attached to this connection. If this is a system connection it will be null.
        /// </summary>
        IApplicationUserState? CurrentUser { get; }

        /// <summary>
        /// Provides an impersonation context to the application domain account.
        /// </summary>
        WindowsImpersonation Impersonation { get; }

        /// <summary>
        /// A list of the domain controllers that are members of the domain that was connected
        /// </summary>
        List<DomainController> DomainControllers { get; }

        /// <summary>
        /// Searches for an Active Directory object by it's SID
        /// </summary>
        /// <param name="sid">The SID in string form to search against</param>
        /// <returns>The matching object in Active Directory, or null</returns>
        IDirectoryEntryAdapter? GetDirectoryEntryBySid(string sid);

        /// <summary>
        /// Searches for an Active Directory object by it's SID
        /// </summary>
        /// <param name="sid">The SID in byte array form to search against</param>
        /// <returns>The matching object in Active Directory, or null</returns>
        IDirectoryEntryAdapter? FindEntryBySID(byte[] sid);



        /// <summary>
        /// Authenticates a login request's credentials against the configured
        /// Active Directory connection in the application settings database.
        /// </summary>
        /// <param name="loginReq">The login request to validate</param>
        /// <returns>The Active Directory user who authenticated, or null if the credentials were invalid, or connection
        /// attempt failed.</returns>
        IADUser? Authenticate(LoginRequest loginReq);


        /// <summary>
        /// Connects to an Active Directory server.
        /// It first checks the database connection status, then retrieves the Active 
        /// Directory settings from the database.Sets the Status property to reflect the
        /// server connectivity.
        /// </summary>
        /// <returns></returns>
        void Connect();

        /// <summary>
        /// Connects to an Active Directory server asynchronously.
        /// It first checks the database connection status, then retrieves the Active 
        /// Directory settings from the database.Sets the Status property to reflect the
        /// server connectivity.
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Collects all deleted object from the Active Directory recycle bin
        /// </summary>
        /// <returns></returns>
        DirectoryEntry GetDeleteObjectsEntry();

        /// <summary>
        /// Returns the directory entry of the given Base distinguished
        /// name. If no base is provided,  the application scope BaseDN setting
        /// will be used
        /// </summary>
        /// <param name="baseDN"></param>
        /// <returns></returns>
        DirectoryEntry GetDirectoryEntry(string? baseDN = null);

        /// <summary>
        /// Restores an Active Directory object from the recycle bin
        /// </summary>
        /// <param name="model">The object to restore</param>
        /// <param name="newOU">The location to restore to</param>
        /// <returns></returns>
        bool RestoreTombstone(IDirectoryEntryAdapter model, IADOrganizationalUnit newOU);

        /// <summary>
        /// Searches the domain for the Distinguished Name provided
        /// </summary>
        /// <param name="dn">The DN to search for</param>
        /// <returns>The matching entry, otherwise null</returns>
        IDirectoryEntryAdapter? GetDirectoryEntryByDN(string dn);
    }
}