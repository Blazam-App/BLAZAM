using BLAZAM.Common.Data;

namespace BLAZAM.Database.Models.Permissions
{
    public enum ActiveDirectoryObjectAction
    {
        Move, Delete, Create, Unassign, Assign, Enable, Disable, Rename,
        Unlock
    }
    public class ObjectAction : AppDbSetBase
    {
        public string Name { get; set; }
        public ActiveDirectoryObjectAction Action { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is ObjectAction flag &&
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
                
                default:
                    return false;
            }
        }
    }
}