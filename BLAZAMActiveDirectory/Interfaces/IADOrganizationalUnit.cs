using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents an Active Directory Organizational Unit with permissions and operations for managing directory entries.
    /// </summary>
    public interface IADOrganizationalUnit : IDirectoryEntryAdapter
    {
        /// <summary>
        /// Gets or sets the name of the organizational unit.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Gets the collection of sub-organizational units contained within this OU.
        /// </summary>
        IEnumerable<IDirectoryEntryAdapter> SubOUs { get; }

        /// <summary>
        /// Gets the sub-organizational units formatted for tree view display.
        /// </summary>
        HashSet<IDirectoryEntryAdapter> TreeViewSubOUs { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to read users in sub-organizational units.
        /// </summary>
        bool CanReadUsersInSubOus { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to create users in this OU.
        /// </summary>
        bool CanCreateUser { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to create users in this OU.
        /// </summary>
        bool CanCreateType(ActiveDirectoryObjectType type);

        /// <summary>
        /// Gets a value indicating whether the current user has permission to create users in sub-organizational units.
        /// </summary>
        bool CanCreateUserInSubOUs { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to read users in this OU.
        /// </summary>
        bool CanReadUsers { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to read non-OU directory entries.
        /// </summary>
        bool CanReadNonOUs { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to read computer objects in this OU.
        /// </summary>
        bool CanReadComputers { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to read group objects in this OU.
        /// </summary>
        bool CanReadGroups { get; }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to read printer objects in this OU.
        /// </summary>
        bool CanReadPrinters { get; }

        /// <summary>
        /// Creates a new Active Directory group in this organizational unit.
        /// </summary>
        /// <param name="containerName">The name of the group to create.</param>
        /// <returns>The newly created <see cref="IADGroup"/> instance.</returns>
        IADGroup CreateGroup(string containerName);

        /// <summary>
        /// Creates a new Active Directory user in this organizational unit.
        /// </summary>
        /// <param name="containerName">The name of the user to create.</param>
        /// <returns>The newly created <see cref="IADUser"/> instance.</returns>
        IADUser CreateUser(string containerName);

        /// <summary>
        /// Creates a new Active Directory contact in this organizational unit.
        /// </summary>
        /// <param name="containerName">The name of the contact to create.</param>
        /// <returns>The newly created <see cref="IADContact"/> instance.</returns>
        IADContact CreateContact(string containerName);

        /// <summary>
        /// Creates a new sub-organizational unit within this OU.
        /// </summary>
        /// <param name="containerName">The name of the organizational unit to create.</param>
        /// <returns>The newly created <see cref="IADOrganizationalUnit"/> instance.</returns>
        IADOrganizationalUnit CreateOU(string containerName);

        /// <summary>
        /// Asynchronously retrieves all child directory entries (sub-OUs) contained in this organizational unit.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing an enumerable collection of child directory entries.</returns>
        Task<IEnumerable<IDirectoryEntryAdapter>> GetChildrenAsync();

        /// <summary>
        /// Asynchronously determines whether this organizational unit contains any child entries.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether children exist.</returns>
        Task<bool> HasChildrenAsync();

        /// <summary>
        /// Creates a new Active Directory printer object in this organizational unit.
        /// </summary>
        /// <param name="containerName">The name of the printer to create.</param>
        /// <param name="uncPath">The UNC path to the printer share.</param>
        /// <param name="shortServerName">The short server name hosting the printer.</param>
        /// <returns>The newly created <see cref="IADPrinter"/> instance.</returns>
        IADPrinter CreatePrinter(string containerName, string uncPath, string shortServerName);

        /// <summary>
        /// Creates a new Active Directory printer object from a shared printer configuration.
        /// </summary>
        /// <param name="sharedPrinter">The shared printer configuration to use for creating the printer object.</param>
        /// <returns>The newly created <see cref="IADPrinter"/> instance.</returns>
        IADPrinter CreatePrinter(SharedPrinter sharedPrinter);
    }
}