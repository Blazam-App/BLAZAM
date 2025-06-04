using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Provides a connection to an Active Directory Domain
    /// </summary>
    public interface IActiveDirectoryContext : IDisposable
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
        /// How many times the connection has failed.
        /// Max value should not exceed 10
        /// </summary>
        int FailedConnectionAttempts { get; set; }

        /// <summary>
        /// The application scoped directory entry root
        /// </summary>
        IDirectoryEntry? AppRootDirectoryEntry { get; }

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
        /// Provides Contacts search functions
        /// </summary>
        IADContactSearcher Contacts { get; }
        /// <summary>
        /// Provides Printer search functions
        /// </summary>
        IADPrinterSearcher Printers { get; }

        /// <summary>
        /// Provides Computer search functions
        /// </summary>
        IADComputerSearcher Computers { get; }
        /// <summary>
        /// Provides BitLocker search functions
        /// </summary>
        IADBitLockerSearcher BitLocker { get; }

        /// <summary>
        /// Called when the connection state of the Active Directory server has
        /// changed
        /// </summary>
        AppDelegate<DirectoryConnectionStatus>? OnStatusChanged { get; set; }

        /// <summary>
        /// The connection settings as gotten from the <see cref="Factory"/>
        /// </summary>
        ADSettings? ConnectionSettings { get; }


        /// <summary>
        /// The current web user attached to this connection. If this is a system connection it will be null.
        /// </summary>
        ActiveDirectoryUserState? CurrentUser { get; set; }

        /// <summary>
        /// Provides an impersonation context to the application domain account.
        /// </summary>
        WindowsImpersonation Impersonation { get; }

        DomainControllerEventLogReader EventLogReader { get; }
        Exception? ConnectionException { get; set; }

        /// <summary>
        /// Searches for an Active Directory object by it's SID
        /// </summary>
        /// <param name="sid">The SID in string form to search against</param>
        /// <returns>The matching object in Active Directory, or null</returns>
        IDirectoryEntryAdapter? FindEntryBySid(string sid);

        /// <summary>
        /// Searches for an Active Directory object by it's SID
        /// </summary>
        /// <param name="sid">The SID in byte array form to search against</param>
        /// <returns>The matching object in Active Directory, or null</returns>
        IDirectoryEntryAdapter? FindEntryBySID(byte[] sid);
        /// <summary>
        /// Searches for an Active Directory object by it's GUID
        /// </summary>
        /// <param name="sid">The GUID in byte array form to search against</param>
        /// <returns>The matching object in Active Directory, or null</returns>
        IDirectoryEntryAdapter? FindEntryByGuid(byte[] guid);
        /// <summary>
        /// Searches for an Active Directory object by it's GUID
        /// </summary>
        /// <param name="sid">The GUID in string form to search against</param>
        /// <returns>The matching object in Active Directory, or null</returns>
        IDirectoryEntryAdapter? FindEntryByGuid(string guid);



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
        AppLdapConnection? Connect();

        /// <summary>
        /// Connects to an Active Directory server asynchronously.
        /// It first checks the database connection status, then retrieves the Active 
        /// Directory settings from the database.Sets the Status property to reflect the
        /// server connectivity.
        /// </summary>
        /// <returns></returns>
        Task<AppLdapConnection?> ConnectAsync();

        /// <summary>
        /// Collects all deleted object from the Active Directory recycle bin
        /// </summary>
        /// <returns></returns>
        IDirectoryEntry GetDeleteObjectsEntry();

        /// <summary>
        /// Returns the directory entry of the given Base distinguished
        /// name. If no base is provided,  the application scope BaseDN setting
        /// will be used
        /// </summary>
        /// <param name="baseDN"></param>
        /// <returns></returns>
        IDirectoryEntry GetDirectoryEntry(string? baseDN = null);

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
        IDirectoryEntryAdapter? GetDirectoryEntryByDN(string? dn);
        Task CancelConnection();
    }
}