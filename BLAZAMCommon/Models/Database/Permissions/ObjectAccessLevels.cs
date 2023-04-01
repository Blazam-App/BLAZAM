namespace BLAZAM.Common.Models.Database.Permissions
{
    public class ObjectAccessLevels
    {
        public static ObjectAccessLevel Deny = new ObjectAccessLevel() { Id = 1, Name = "Deny", Level = 10 };
        public static ObjectAccessLevel Read = new ObjectAccessLevel() { Id = 2, Name = "Read", Level = 1000 };
        public static List<ObjectAccessLevel> Levels = new List<ObjectAccessLevel>() {
            new ObjectAccessLevel() { Id=1,Name="Deny",Level = 10},
            new ObjectAccessLevel() { Id=2,Name="Read",Level=1000}
        };

    }
}
