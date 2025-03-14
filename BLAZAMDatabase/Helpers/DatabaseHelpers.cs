using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;

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

            //var Name = action.ToString();
            switch (type)
            {
                case ActiveDirectoryObjectType.User:
                case ActiveDirectoryObjectType.Computer:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Unlock:
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Enable:
                        case ActiveDirectoryObjectAction.Disable:
                        case ActiveDirectoryObjectAction.Rename:
                        case ActiveDirectoryObjectAction.SetPassword:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Group:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Unassign:
                        case ActiveDirectoryObjectAction.Assign:
                        case ActiveDirectoryObjectAction.Rename:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Printer:
                case ActiveDirectoryObjectType.OU:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Move:
                        case ActiveDirectoryObjectAction.Delete:
                        case ActiveDirectoryObjectAction.Create:
                        case ActiveDirectoryObjectAction.Rename:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.BitLocker:
                    switch (action)
                    {
                        case ActiveDirectoryObjectAction.Delete:
                            return true;
                        default:
                            return false;
                    }

                default:
                    return false;
            }
        }

        public static bool IsActionAppropriateForObject(this ObjectAction action, ActiveDirectoryObjectType type) => IsActionAppropriateForObject(action.Action, type);
    }
}
