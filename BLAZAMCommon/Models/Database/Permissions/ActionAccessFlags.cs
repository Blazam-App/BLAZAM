namespace BLAZAM.Common.Models.Database.Permissions
{
    public class ActionAccessFlags
    {
        public static List<ActionAccessFlag> Flags = new List<ActionAccessFlag>()
        {

                  new ActionAccessFlag() { Id = 1, Name = "Assign" },
                  new ActionAccessFlag() { Id = 2, Name = "UnAssign" },
                  new ActionAccessFlag() { Id = 3, Name = "Unlock" },
                  new ActionAccessFlag() { Id = 4, Name = "Enable" },
                  new ActionAccessFlag() { Id = 5, Name = "Disable" },
                  new ActionAccessFlag() { Id = 6, Name = "Rename" },
                  new ActionAccessFlag() { Id = 7, Name = "Move" },
                  new ActionAccessFlag() { Id = 8, Name = "Create" },
                  new ActionAccessFlag() { Id = 9, Name = "Delete" },
                  new ActionAccessFlag() { Id = 10, Name = "Set Password" }
        };
        public static ActionAccessFlag Assign = new ActionAccessFlag() { Id = 1, Name = "Assign" };
        public static ActionAccessFlag UnAssign = new ActionAccessFlag() { Id = 2, Name = "UnAssign" };
        public static ActionAccessFlag Unlock = new ActionAccessFlag() { Id = 3, Name = "Unlock" };
        public static ActionAccessFlag Enable = new ActionAccessFlag() { Id = 4, Name = "Enable" };
        public static ActionAccessFlag Disable = new ActionAccessFlag() { Id = 5, Name = "Disable" };
        public static ActionAccessFlag Rename = new ActionAccessFlag() { Id = 6, Name = "Rename" };
        public static ActionAccessFlag Move = new ActionAccessFlag() { Id = 7, Name = "Move" };
        public static ActionAccessFlag Create = new ActionAccessFlag() { Id = 8, Name = "Create" };
        public static ActionAccessFlag Delete = new ActionAccessFlag() { Id = 9, Name = "Delete" };
        public static ActionAccessFlag SetPassword = new ActionAccessFlag() { Id = 10, Name = "Set Password" };
    }
}