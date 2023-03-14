namespace BLAZAM.Database.Models.Database.Permissions
{
    public class ActionAccessFlags
    {
        public static List<ActionAccessFlag> Flags = new List<ActionAccessFlag>()
        {

                  new ActionAccessFlag() { ActionAccessFlagId = 1, Name = "Assign" },
                  new ActionAccessFlag() { ActionAccessFlagId = 2, Name = "UnAssign" },
                  new ActionAccessFlag() { ActionAccessFlagId = 3, Name = "Unlock" },
                  new ActionAccessFlag() { ActionAccessFlagId = 4, Name = "Enable" },
                  new ActionAccessFlag() { ActionAccessFlagId = 5, Name = "Disable" },
                  new ActionAccessFlag() { ActionAccessFlagId = 6, Name = "Rename" },
                  new ActionAccessFlag() { ActionAccessFlagId = 7, Name = "Move" },
                  new ActionAccessFlag() { ActionAccessFlagId = 8, Name = "Create" },
                  new ActionAccessFlag() { ActionAccessFlagId = 9, Name = "Delete" },
                  new ActionAccessFlag() { ActionAccessFlagId = 10, Name = "Set Password" }
        };
        public static ActionAccessFlag Assign = new ActionAccessFlag() { ActionAccessFlagId = 1, Name = "Assign" };
        public static ActionAccessFlag UnAssign = new ActionAccessFlag() { ActionAccessFlagId = 2, Name = "UnAssign" };
        public static ActionAccessFlag Unlock = new ActionAccessFlag() { ActionAccessFlagId = 3, Name = "Unlock" };
        public static ActionAccessFlag Enable = new ActionAccessFlag() { ActionAccessFlagId = 4, Name = "Enable" };
        public static ActionAccessFlag Disable = new ActionAccessFlag() { ActionAccessFlagId = 5, Name = "Disable" };
        public static ActionAccessFlag Rename = new ActionAccessFlag() { ActionAccessFlagId = 6, Name = "Rename" };
        public static ActionAccessFlag Move = new ActionAccessFlag() { ActionAccessFlagId = 7, Name = "Move" };
        public static ActionAccessFlag Create = new ActionAccessFlag() { ActionAccessFlagId = 8, Name = "Create" };
        public static ActionAccessFlag Delete = new ActionAccessFlag() { ActionAccessFlagId = 9, Name = "Delete" };
        public static ActionAccessFlag SetPassword = new ActionAccessFlag() { ActionAccessFlagId = 10, Name = "Set Password" };
    }
}