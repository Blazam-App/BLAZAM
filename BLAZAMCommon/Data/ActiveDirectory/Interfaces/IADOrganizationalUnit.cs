using BLAZAM.Common.Models.Database.Permissions;
using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADOrganizationalUnit : IDirectoryEntryAdapter
    {
        string Name { get; set; }
        List<PermissionMap> InheritedPermissionMappings { get; }
        IQueryable<PermissionMap> AppliedPermissionMappings { get; }
        List<PermissionMap> DirectPermissionMappings { get; }
        IQueryable<PermissionMap> OffspringPermissionMappings { get; }
        IEnumerable<IADOrganizationalUnit> Children { get; }
        IQueryable<IADUser> ChildUsers { get; }
        IADUser CreateUser(string containerName);
        IADOrganizationalUnit CreateOU(string containerName);
        Task<IEnumerable<IADOrganizationalUnit>> GetChildrenAsync();
        Task<bool> HasChildrenAsync();
    }
}