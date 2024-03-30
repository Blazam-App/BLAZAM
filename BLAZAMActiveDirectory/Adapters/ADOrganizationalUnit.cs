using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.Contracts;
using System.DirectoryServices;
using System.Reflection.PortableExecutable;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADOrganizationalUnit : DirectoryEntryAdapter, IADOrganizationalUnit
    {
        private IEnumerable<IADOrganizationalUnit>? childOUCache;
        //private IQueryable<IADUser>? childUserCache;
        //private IQueryable<IADComputer>? childComputerCache;
        //private IQueryable<IADGroup>? childGroupCache;



        public async Task<bool> HasChildrenAsync()
        {
            return await Task.Run(() =>
            {
                return HasChildren;
            });
        }
        public async Task<IEnumerable<IADOrganizationalUnit>> GetChildrenAsync()
        {
            return await Task.Run(() =>
            {
                return SubOUs;
            });
        }

        public HashSet<IADOrganizationalUnit> CachedTreeViewSubOUs { get; private set; } = new();

        public HashSet<IADOrganizationalUnit> TreeViewSubOUs
        {
            get
            {
                CachedTreeViewSubOUs = SubOUs.ToHashSet();
                return CachedTreeViewSubOUs;
            }
        }

        public IEnumerable<IADOrganizationalUnit> SubOUs
        {
            get
            {
                if (childOUCache == null)
                    childOUCache = Directory.OUs.FindSubOusByDN(DN).OrderBy(ou => ou.CanonicalName).AsQueryable();


                return childOUCache;
            }
        }



        public override string SearchUri => "/search/" + DN;

        public override string? CanonicalName
        {
            get => Name;
            set => Name = value;
        }
        public string? Name
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
        public List<PermissionMapping> InheritedPermissionMappings
        {
            get
            {
                return AppliedPermissionMappings.Where(m => !m.OU.Equals(DN)).ToList();
            }
        }
        public List<PermissionMapping> DirectPermissionMappings
        {
            get
            {
               
                   return AppliedPermissionMappings.Where(m => m.OU.Equals(DN)).ToList();
 
            }
        }

        private IQueryable<PermissionMapping> _appliedPermissionMappings;

        public IQueryable<PermissionMapping> AppliedPermissionMappings
        {
            get
            {
                if (_appliedPermissionMappings == null)
                {

                    _appliedPermissionMappings = DbFactory.CreateDbContext().PermissionMap.Include(m => m.PermissionDelegates).Where(m => DN.Contains(m.OU)).OrderByDescending(m => m.OU.Length);
                }
                return _appliedPermissionMappings;
            }
        }
        private IQueryable<PermissionMapping> _offspringPermissionMappings;
        public IQueryable<PermissionMapping> OffspringPermissionMappings
        {
            get
            {
                if (_offspringPermissionMappings == null)
                {

                    _offspringPermissionMappings = DbFactory.CreateDbContext().PermissionMap.Include(m => m.PermissionDelegates).Where(m => m.OU.Contains(DN) && m.OU != DN).OrderByDescending(m => m.OU.Length);
                }
                return _offspringPermissionMappings;
            }
        }




        /// <summary>
        /// Creates a new OU under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The name of the new ou</param>
        /// <returns>An uncommited organizational unit</returns>
        public IADOrganizationalUnit CreateOU(string containerName)
        {
            EnsureDirectoryEntry();
            IADOrganizationalUnit newOU = new ADOrganizationalUnit();

            newOU.Parse(directoryEntry: DirectoryEntry.Children.Add("OU=" + containerName.Trim(), "OrganizationalUnit"), directory: Directory);
            newOU.NewEntry = true;
            return newOU;
        }
        /// <summary>
        /// Creates a new user under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new user</param>
        /// <returns>An uncommited user</returns>
        public IADUser CreateUser(string containerName)
        {
            var fullContainerName = "CN=" + containerName.Trim().Replace(",", "\\,");
            try
            {
                IADUser newUser = new ADUser();
                EnsureDirectoryEntry();
                newUser.Parse(directoryEntry: DirectoryEntry.Children.Add(fullContainerName, "user"), directory: Directory);
                newUser.NewEntry = true;
                newUser.Enabled = true;
                return newUser;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Error while attempting to create user: " + fullContainerName + " {@Error}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Creates a new group under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new group</param>
        /// <returns>An uncommited group</returns>
        public IADGroup CreateGroup(string containerName)
        {
            EnsureDirectoryEntry();
            IADGroup newGroup = new ADGroup();
            if (DirectoryEntry == null)
                DirectoryEntry = searchResult?.GetDirectoryEntry();
            newGroup.Parse(directoryEntry: DirectoryEntry.Children.Add("CN=" + containerName.Trim(), "group"), directory: Directory);
            newGroup.NewEntry = true;
            newGroup.SamAccountName = containerName.Trim();
            return newGroup;

        }

        /// <summary>
        /// Creates a new printer under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new printer</param>
        /// <returns>An uncommited printer</returns>
        public IADPrinter CreatePrinter(string containerName, string uncPath, string shortServerName)
        {

            IADPrinter newPrinter = new ADPrinter();
            if (DirectoryEntry == null)
                DirectoryEntry = searchResult?.GetDirectoryEntry();
            newPrinter.Parse(directoryEntry: DirectoryEntry.Children.Add("CN=" + shortServerName + "-" + containerName.Trim(), "printQueue"), directory: Directory);
            newPrinter.NewEntry = true;
            newPrinter.UncName = uncPath;
            newPrinter.PrinterName = containerName.Trim();
            newPrinter.ShortServerName = shortServerName;
            return newPrinter;

        }
        /// <summary>
        /// Creates a new printer under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="sharedPrinter">The sharedPrinter to be added</param>
        /// <returns>An uncommited printer</returns>
        public IADPrinter CreatePrinter(SharedPrinter sharedPrinter)
        {
            IADPrinter newPrinter = new ADPrinter();
            if (DirectoryEntry == null)
                DirectoryEntry = searchResult?.GetDirectoryEntry();
            newPrinter.Parse(directoryEntry: DirectoryEntry.Children.Add("CN=" + sharedPrinter.Host.CanonicalName + "-" + sharedPrinter.ShareName.Trim(), "printQueue"), directory: Directory);
            newPrinter.NewEntry = true;
            newPrinter.UncName = "\\\\" + sharedPrinter.Host.CanonicalName + "\\" + sharedPrinter.ShareName;
            newPrinter.PrinterName = sharedPrinter.Name.Trim();
            newPrinter.ShortServerName = sharedPrinter.Host.CanonicalName;
            newPrinter.ServerName = sharedPrinter.Host.CanonicalName;
            newPrinter.VersionNumber = 4;
            newPrinter.Location = sharedPrinter.Location.Trim();
            newPrinter.DriverName = sharedPrinter.DriverName.Trim();
            newPrinter.PortName = sharedPrinter.PortName.Trim();
            return newPrinter;
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