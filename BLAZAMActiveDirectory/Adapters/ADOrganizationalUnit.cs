using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Logger;
using System.Web;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADOrganizationalUnit : DirectoryEntryAdapter, IADOrganizationalUnit
    {
        private IEnumerable<IADOrganizationalUnit>? childOUCache;


        public async Task<bool> HasChildrenAsync()
        {
            return await Task.Run(() =>
            {
                return HasChildren;
            });
        }
        public async Task<IEnumerable<IDirectoryEntryAdapter>> GetChildrenAsync()
        {
            return await Task.Run(() =>
            {
                return SubOUs;
            });
        }

        public HashSet<IDirectoryEntryAdapter> TreeViewSubOUs
        {
            get
            {
                return SubOUs.ToHashSet();

            }
        }

        public IEnumerable<IDirectoryEntryAdapter> SubOUs
        {
            get
            {
                if (childOUCache == null)
                    childOUCache = Directory.OUs.FindSubOusByDN(DN).OrderBy(ou => ou.CanonicalName).AsQueryable();


                return childOUCache;
            }
        }



        public override string SearchUri => "/view/" + HttpUtility.UrlEncode(DN);

        public override string? CanonicalName
        {
            get => Name;
            set => Name = value;
        }
        public string? Name
        {
            get
            {
                return GetStringAttribute("name");
            }
            set
            {
                SetAttribute("name", value);
            }
        }

        public virtual bool CanReadUsersInSubOus
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
                ))),
                p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
                ))),
                true
                );
            }

        }
        public virtual bool CanReadNonOUs
        {
            get
            {
                if (CanReadUsers) return true;
                if (CanReadGroups) return true;
                if (CanReadComputers) return true;
                if (CanReadPrinters) return true;
                return false;
            }
        }
        public virtual bool CanReadPrinters
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.Printer &&
              om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.Printer &&
              om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
              )))
              );
            }

        }
        public virtual bool CanReadComputers
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.Computer &&
              om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.Computer &&
              om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
              )))
              );
            }

        }
        public virtual bool CanReadGroups
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.Group &&
              om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.Group &&
              om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
              )))
              );
            }

        }
        public virtual bool CanReadUsers
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.User &&
              om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al =>
              al.ObjectMap.Any(om =>
              om.ObjectType == ActiveDirectoryObjectType.User &&
              om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
              )))
              );
            }

        }
        public virtual bool CanCreateUser
        {
            get
            {
                return HasActionPermission(ObjectActions.Create, ActiveDirectoryObjectType.User);
            }

        }
        public virtual bool CanCreateUserInSubOUs
        {
            get
            {

                return HasPermission(p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ActionMap.Any(am =>
                am.ObjectType == ActiveDirectoryObjectType.User &&
                am.ObjectAction.Id == ObjectActions.Create.Id &&
                am.AllowOrDeny == true))),
                p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ActionMap.Any(am =>
                am.ObjectType == ActiveDirectoryObjectType.User &&
                am.ObjectAction.Id == ObjectActions.Create.Id &&
                am.AllowOrDeny == false))),
                true
                );

            }

        }

        /// <summary>
        /// Creates a new OU under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The name of the new ou</param>
        /// <returns>An uncommitted organizational unit</returns>
        public IADOrganizationalUnit CreateOU(string containerName)
        {
            EnsureDirectoryEntry();
            IADOrganizationalUnit newOU = new ADOrganizationalUnit();

            newOU.Parse(directoryEntry: DirectoryEntry!.Children.Add("OU=" + containerName.Trim(), "OrganizationalUnit"), directory: Directory);
            newOU.NewEntry = true;
            return newOU;
        }
        /// <summary>
        /// Creates a new user under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new user</param>
        /// <returns>An uncommitted user</returns>
        public IADUser CreateUser(string containerName)
        {

            EnsureDirectoryEntry();

            var fullContainerName = "CN=" + containerName.Trim().Replace(",", "\\,");
            try
            {
                IADUser newUser = new ADUser();
                newUser.Parse(directoryEntry: DirectoryEntry!.Children.Add(fullContainerName, "user"), directory: Directory);
                newUser.NewEntry = true;
                newUser.Enabled = true;
                return newUser;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error while attempting to create user in {@ContainerName}", fullContainerName);
                throw;
            }
        }
        /// <summary>
        /// Creates a new contact under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new contact</param>
        /// <returns>An uncommitted contact</returns>
        public IADContact CreateContact(string containerName)
        {

            EnsureDirectoryEntry();

            var fullContainerName = "CN=" + containerName.Trim().Replace(",", "\\,");
            try
            {
                ADContact newContact = new ADContact();
                newContact.Parse(directoryEntry: DirectoryEntry!.Children.Add(fullContainerName, "contact"), directory: Directory);
                newContact.NewEntry = true;
                return newContact;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error while attempting to create contact in {@ContainerName}", fullContainerName);

                throw;
            }
        }

        /// <summary>
        /// Creates a new group under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new group</param>
        /// <returns>An uncommitted group</returns>
        public IADGroup CreateGroup(string containerName)
        {
            EnsureDirectoryEntry();
            IADGroup newGroup = new ADGroup();

            newGroup.Parse(directoryEntry: DirectoryEntry!.Children.Add("CN=" + containerName.Trim(), "group"), directory: Directory);
            newGroup.NewEntry = true;
            newGroup.SAMAccountName = containerName.Trim();
            return newGroup;

        }

        /// <summary>
        /// Creates a new printer under this OU. Note that the returned Directory object
        /// must execute CommitChanges() to actually create the object in Active
        /// Directory.
        /// </summary>
        /// <param name="containerName">The container name of the new printer</param>
        /// <returns>An uncommitted printer</returns>
        public IADPrinter CreatePrinter(string containerName, string uncPath, string shortServerName)
        {
            EnsureDirectoryEntry();

            IADPrinter newPrinter = new ADPrinter();
            newPrinter.Parse(directoryEntry: DirectoryEntry!.Children.Add("CN=" + shortServerName + "-" + containerName.Trim(), "printQueue"), directory: Directory);
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
            EnsureDirectoryEntry();

            IADPrinter newPrinter = new ADPrinter();
            newPrinter.Parse(directoryEntry: DirectoryEntry!.Children.Add("CN=" + sharedPrinter.Host.CanonicalName + "-" + sharedPrinter.ShareName.Trim(), "printQueue"), directory: Directory);
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
            if (obj is IADOrganizationalUnit otherOU && otherOU.DN == DN)
            {
                return true;
            }

            return false;
        }
    }

}