using BLAZAM.Helpers;

namespace BLAZAM.Database.Models.Permissions
{
    public class ObjectActions
    {
        public static List<ObjectAction> Flags => typeof(ObjectActions).GetStaticProperties<ObjectAction>();
        
        public static readonly ObjectAction Assign = new() {
            Id = 1,
            Name = "Assign" ,
            Action=ActiveDirectoryObjectAction.Assign
        };
        public static readonly ObjectAction UnAssign = new() {
            Id = 2,
            Name = "UnAssign",
            Action = ActiveDirectoryObjectAction.Unassign
        };
        public static readonly ObjectAction Unlock = new() { 
            Id = 3,
            Name = "Unlock",
            Action = ActiveDirectoryObjectAction.Unlock
        };
        public static readonly ObjectAction Enable = new() {
            Id = 4,
            Name = "Enable",
            Action = ActiveDirectoryObjectAction.Enable
        };
        public static readonly ObjectAction Disable = new() {
            Id = 5,
            Name = "Disable",
            Action = ActiveDirectoryObjectAction.Disable
        };
        public static readonly ObjectAction Rename = new() {
            Id = 6,
            Name = "Rename",
            Action = ActiveDirectoryObjectAction.Rename
        };
        public static readonly ObjectAction Move = new() {
            Id = 7,
            Name = "Move",
            Action = ActiveDirectoryObjectAction.Move
        };
        public static readonly ObjectAction Create = new() {
            Id = 8,
            Name = "Create",
            Action = ActiveDirectoryObjectAction.Create
        };
        public static readonly ObjectAction Delete = new() {
            Id = 9, 
            Name = "Delete",
            Action = ActiveDirectoryObjectAction.Delete
        };
        public static readonly ObjectAction SetPassword = new() {
            Id = 10, 
            Name = "Set Password",
            Action = ActiveDirectoryObjectAction.SetPassword
        };
    }
}