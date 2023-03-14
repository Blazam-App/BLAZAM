using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Database.Models.Database.Permissions
{
    public class ActionAccessFlag
    {
        public int ActionAccessFlagId { get; set; }
        public string Name { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ActionAccessFlag flag &&
                   Name == flag.Name;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public bool IsActionAppropriateForObject(ActiveDirectoryObjectType type)
        {
            switch (type)
            {
                case ActiveDirectoryObjectType.User:
                    return true;
                case ActiveDirectoryObjectType.Computer:
                    switch (Name)
                    {
                        case "Move":
                        case "Delete":
                        case "Create":
                        case "UnAssign":
                        case "Assign":
                        case "Enable":
                        case "Disable":
                        case "Rename":
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

                default:
                    return false;
            }
        }
    }
}