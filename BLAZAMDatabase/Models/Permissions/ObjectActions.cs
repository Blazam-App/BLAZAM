namespace BLAZAM.Database.Models.Permissions
{
    public class ObjectActions
    {
        public static List<ObjectAction> Flags = new()
        {

                  new ObjectAction() { Id = 1, Name = "Assign" },
                  new ObjectAction() { Id = 2, Name = "UnAssign" },
                  new ObjectAction() { Id = 3, Name = "Unlock" },
                  new ObjectAction() { Id = 4, Name = "Enable" },
                  new ObjectAction() { Id = 5, Name = "Disable" },
                  new ObjectAction() { Id = 6, Name = "Rename" },
                  new ObjectAction() { Id = 7, Name = "Move" },
                  new ObjectAction() { Id = 8, Name = "Create" },
                  new ObjectAction() { Id = 9, Name = "Delete" },
                  new ObjectAction() { Id = 10, Name = "Set Password" }
        };
        public static ObjectAction Assign = new() { Id = 1, Name = "Assign" };
        public static ObjectAction UnAssign = new() { Id = 2, Name = "UnAssign" };
        public static ObjectAction Unlock = new() { Id = 3, Name = "Unlock" };
        public static ObjectAction Enable = new() { Id = 4, Name = "Enable" };
        public static ObjectAction Disable = new() { Id = 5, Name = "Disable" };
        public static ObjectAction Rename = new() { Id = 6, Name = "Rename" };
        public static ObjectAction Move = new() { Id = 7, Name = "Move" };
        public static ObjectAction Create = new() { Id = 8, Name = "Create" };
        public static ObjectAction Delete = new() { Id = 9, Name = "Delete" };
        public static ObjectAction SetPassword = new() { Id = 10, Name = "Set Password" };
    }
}