﻿
using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADOrganizationalUnit : IDirectoryEntryAdapter

    {
        /// <summary>
        /// Indicates whether this OU is expanded
        /// in the ui withing a tree view
        /// </summary>
        bool IsExpanded { get; set; }

        string Name { get; set; }
        List<PermissionMapping> InheritedPermissionMappings { get; }
        IQueryable<PermissionMapping> AppliedPermissionMappings { get; }
        List<PermissionMapping> DirectPermissionMappings { get; }
        IQueryable<PermissionMapping> OffspringPermissionMappings { get; }
        IEnumerable<IADOrganizationalUnit> Children { get; }
        IQueryable<IADUser> ChildUsers { get; }
        bool IsLoadingChildren { get; set; }

        IADGroup CreateGroup(string containerName);
        IADUser CreateUser(string containerName);
        IADOrganizationalUnit CreateOU(string containerName);
        Task<IEnumerable<IADOrganizationalUnit>> GetChildrenAsync();
        bool HasChildren();
        Task<bool> HasChildrenAsync();
    }
}