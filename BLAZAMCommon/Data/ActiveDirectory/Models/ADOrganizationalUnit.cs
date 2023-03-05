using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Graph;
using System.DirectoryServices;
using System.Reflection.PortableExecutable;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public class ADOrganizationalUnit : DirectoryModel, IADOrganizationalUnit
    {
        private IEnumerable<IADOrganizationalUnit>? childrenCache;
        private IQueryable<IADUser>? childUserCache;
        private IQueryable<IADComputer>? childComputerCache;
        private IQueryable<IADGroup>? childGroupCache;

     
        public async Task<bool> HasChildrenAsync()
        {
            return await Task.Run(() => {
                return Children.Any();
            });
        }
        public async Task<IEnumerable<IADOrganizationalUnit>> GetChildrenAsync()
        {
            return await Task.Run(() => {
                return Children;
            });
        }
        public IEnumerable<IADOrganizationalUnit> Children
        {
            get
            {

                if (childrenCache == null)
                    childrenCache = Directory.OUs.FindSubOusByDN(DN).OrderBy(ou=>ou.CanonicalName).AsQueryable();
                return childrenCache;
            }
        }
        public IQueryable<IADUser> ChildUsers
        {
            get
            {
                if (childUserCache == null)

                    childUserCache = Directory.OUs.FindSubUsersByDN(DN).OrderBy(ou => ou.CanonicalName).AsQueryable();
                return childUserCache;

            }
        }
        public IQueryable<IADComputer> ChildComputers
        {
            get
            {
                if (childComputerCache == null)

                    childComputerCache = Directory.OUs.FindSubComputerByDN(DN).OrderBy(ou => ou.CanonicalName).AsQueryable();
                return childComputerCache;

            }
        }
        public IQueryable<IADGroup> ChildGroups
        {
            get
            {
                if (childGroupCache == null)

                    childGroupCache = Directory.OUs.FindSubGroupsByDN(DN).OrderBy(ou => ou.CanonicalName).AsQueryable();
                return childGroupCache;

            }
        }
        public override string? CanonicalName
        {
            get => Name;
            set => Name=value;
        }
        public string Name
        {
            get
            {
                return GetStringProperty("name");
            }
            set
            {
                SetProperty("name", value);
            }
        }
        public List<PermissionMap> InheritedPermissionMappings
        {
            get
            {
                return this.AppliedPermissionMappings.Where(m => !m.OU.Equals(DN)).ToList();
            }
        }
        public List<PermissionMap> DirectPermissionMappings
        {
            get
            {
                return this.AppliedPermissionMappings.Where(m => m.OU.Equals(DN)).ToList();
            }
        }


        public IQueryable<PermissionMap> AppliedPermissionMappings
        {
            get
            {


                return Context.PermissionMap.Include(m => m.PermissionDelegates).Where(m => DN.Contains(m.OU)).OrderByDescending(m => m.OU.Length);
            }
        }

        public IQueryable<PermissionMap> OffspringPermissionMappings
        {
            get
            {


                return Context.PermissionMap.Include(m => m.PermissionDelegates).Where(m => m.OU.Contains(DN) && m.OU != DN).OrderByDescending(m => m.OU.Length);
            }
        }
        /// <summary>
        /// Creates a new group under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>An uncommited group</returns>
        public IADGroup CreateGroup(string containerName)
        {
            IADGroup newOU = new ADGroup();

            newOU.Parse(DirectoryEntry.Children.Add("CN=" + containerName.Trim(), "group"), Directory);
            newOU.NewEntry = true;
            return newOU;
        }

        /// <summary>
        /// Creates a new OU under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>An uncommited organizational unit</returns>
        public IADOrganizationalUnit CreateOU(string containerName)
        {
            IADOrganizationalUnit newOU = new ADOrganizationalUnit();

            newOU.Parse(DirectoryEntry.Children.Add("OU=" + containerName.Trim(), "OrganizationalUnit"), Directory);
            newOU.NewEntry = true;
            return newOU;
        }
        /// <summary>
        /// Creates a new user under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>An uncommited user</returns>
        public IADUser CreateUser(string containerName)
        {

            IADUser newUser = new ADUser();
            if (DirectoryEntry == null)
                DirectoryEntry = searchResult.GetDirectoryEntry();
            newUser.Parse(DirectoryEntry.Children.Add("CN=" + containerName.Trim(), "user"),Directory);
            newUser.NewEntry = true;
            return newUser;

        }
        public override int GetHashCode()
        {
            return DN.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is IADOrganizationalUnit otherOU)
            {
                if (otherOU.DN == DN)
                {
                    return true;
                }
            }
            return false;
        }
    }

}