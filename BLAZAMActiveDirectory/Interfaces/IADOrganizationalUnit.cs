
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADOrganizationalUnit : IDirectoryEntryAdapter

    {

        string? Name { get; set; }
        List<PermissionMapping> InheritedPermissionMappings { get; }
        IQueryable<PermissionMapping> AppliedPermissionMappings { get; }
        List<PermissionMapping> DirectPermissionMappings { get; }
        IQueryable<PermissionMapping> OffspringPermissionMappings { get; }
        IEnumerable<IADOrganizationalUnit> SubOUs { get; }

        HashSet<IADOrganizationalUnit> CachedTreeViewSubOUs { get;}
        HashSet<IADOrganizationalUnit> TreeViewSubOUs { get; }
        bool CanReadInSubOus { get; }
        bool CanCreateUser { get; }

        IADGroup CreateGroup(string containerName);
        IADUser CreateUser(string containerName);
        IADOrganizationalUnit CreateOU(string containerName);
        Task<IEnumerable<IADOrganizationalUnit>> GetChildrenAsync();
        Task<bool> HasChildrenAsync();
        IADPrinter CreatePrinter(string containerName, string uncPath, string shortServerName);
        IADPrinter CreatePrinter(SharedPrinter sharedPrinter);
    }
}