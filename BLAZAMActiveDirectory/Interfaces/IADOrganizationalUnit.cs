
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADOrganizationalUnit : IDirectoryEntryAdapter

    {

        string? Name { get; set; }
        
        IEnumerable<IDirectoryEntryAdapter> SubOUs { get; }

        HashSet<IDirectoryEntryAdapter> CachedTreeViewSubOUs { get;}
        HashSet<IDirectoryEntryAdapter> TreeViewSubOUs { get; }
        bool CanReadUsersInSubOus { get; }
        bool CanCreateUser { get; }
        bool CanCreateUserInSubOUs { get; }
        bool CanReadUsers { get; }
        bool CanReadNonOUs { get; }
        bool CanReadComputers { get; }
        bool CanReadGroups { get; }
        bool CanReadPrinters { get; }

        IADGroup CreateGroup(string containerName);
        IADUser CreateUser(string containerName);
        IADOrganizationalUnit CreateOU(string containerName);
        Task<IEnumerable<IDirectoryEntryAdapter>> GetChildrenAsync();
        Task<bool> HasChildrenAsync();
        IADPrinter CreatePrinter(string containerName, string uncPath, string shortServerName);
        IADPrinter CreatePrinter(SharedPrinter sharedPrinter);
    }
}