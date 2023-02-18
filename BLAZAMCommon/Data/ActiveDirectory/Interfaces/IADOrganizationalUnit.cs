using BLAZAM.Common.Models.Database.Permissions;
using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADOrganizationalUnit : IDirectoryModel
    {
        string Name { get; set; }
        List<PrivilegeMap> InheritedPermissionMappings { get; }
        IQueryable<PrivilegeMap> AppliedPermissionMappings { get; }
        List<PrivilegeMap> DirectPermissionMappings { get; }
        IQueryable<PrivilegeMap> OffspringPermissionMappings { get; }
        IEnumerable<IADOrganizationalUnit> Children { get; }
        IQueryable<IADUser> ChildUsers { get; }
        IADUser CreateUser(string containerName);
        IADOrganizationalUnit CreateOU(string containerName);
        Task<IEnumerable<IADOrganizationalUnit>> GetChildrenAsync();
        Task<bool> HasChildrenAsync();
    }
}