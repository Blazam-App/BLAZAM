using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class DatabaseHelpers
    {
        public static long GetMembersHash(this IEnumerable<AppUser> members)
        {
            long hash = 0;
            foreach (var member in members)
            {
                hash += member.Username.GetAppHashCode();
            }
            return hash;
        }
        public static List<ActiveDirectoryObjectAction> ToList(this ActiveDirectoryObjectAction _enum)
        {
            return Enum.GetValues(typeof(ActiveDirectoryObjectAction)).Cast<ActiveDirectoryObjectAction>().ToList();
        }

        public static bool IsActionAppropriateForObject(this ActiveDirectoryObjectAction action, ActiveDirectoryObjectType type)
        {

            var Name = action.ToString();
            switch (type)
            {
                case ActiveDirectoryObjectType.User:
                case ActiveDirectoryObjectType.Computer:
                    switch (Name)
                    {
                        case "Lock":
                        case "Unlock":
                        case "Move":
                        case "Delete":
                        case "Create":
                        case "Enable":
                        case "Disable":
                        case "Rename":
                        case "SetPassword":
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Group:
                    switch (Name)
                    {
                        case "Move":
                        case "Delete":
                        case "Create":
                        case "Unassign":
                        case "Rename":
                        case "Assign":
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Printer:
                case ActiveDirectoryObjectType.OU:
                    switch (Name)
                    {
                        case "Move":
                        case "Delete":
                        case "Rename":
                        case "Create":
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.BitLocker:
                    switch (Name)
                    {
                        case "Delete":
                            return true;
                        default:
                            return false;
                    }

                default:
                    return false;
            }
        }

        public static bool IsActionAppropriateForObject(this ObjectAction action, ActiveDirectoryObjectType type)
        {
            var Name = action.Name;
            switch (type)
            {
                case ActiveDirectoryObjectType.User:
                case ActiveDirectoryObjectType.Computer:
                    switch (Name)
                    {
                        case "Lock":
                        case "Unlock":
                        case "Move":
                        case "Delete":
                        case "Create":
                        case "Enable":
                        case "Disable":
                        case "Rename":
                        case "Set Password":
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Group:
                    switch (Name)
                    {
                        case "Move":
                        case "Delete":
                        case "Create":
                        case "UnAssign":
                        case "Rename":
                        case "Assign":
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Printer:
                case ActiveDirectoryObjectType.OU:
                    switch (Name)
                    {
                        case "Move":
                        case "Delete":
                        case "Rename":
                        case "Create":
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.BitLocker:
                    switch (Name)
                    {
                        case "Delete":
                            return true;
                        default:
                            return false;
                    }

                default:
                    return false;
            }
        }
    }
}
