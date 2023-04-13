namespace BLAZAM.Database.Models.Permissions
{
    public class ObjectActions
    {
        public static List<ObjectAction> Flags = new List<ObjectAction>()
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
        public static ObjectAction Assign = new ObjectAction() { Id = 1, Name = "Assign" };
        public static ObjectAction UnAssign = new ObjectAction() { Id = 2, Name = "UnAssign" };
        public static ObjectAction Unlock = new ObjectAction() { Id = 3, Name = "Unlock" };
        public static ObjectAction Enable = new ObjectAction() { Id = 4, Name = "Enable" };
        public static ObjectAction Disable = new ObjectAction() { Id = 5, Name = "Disable" };
        public static ObjectAction Rename = new ObjectAction() { Id = 6, Name = "Rename" };
        public static ObjectAction Move = new ObjectAction() { Id = 7, Name = "Move" };
        public static ObjectAction Create = new ObjectAction() { Id = 8, Name = "Create" };
        public static ObjectAction Delete = new ObjectAction() { Id = 9, Name = "Delete" };
        public static ObjectAction SetPassword = new ObjectAction() { Id = 10, Name = "Set Password" };
    }
}