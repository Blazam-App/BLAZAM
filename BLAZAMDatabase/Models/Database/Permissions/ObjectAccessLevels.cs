namespace BLAZAM.Database.Models.Database.Permissions
{
    public class ObjectAccessLevels
    {
        public static ObjectAccessLevel Deny = new ObjectAccessLevel() { ObjectAccessLevelId = 1, Name = "Deny", Level = 10 };
        public static ObjectAccessLevel Read = new ObjectAccessLevel() { ObjectAccessLevelId = 2, Name = "Read", Level = 1000 };
        public static List<ObjectAccessLevel> Levels = new List<ObjectAccessLevel>() {
            new ObjectAccessLevel() { ObjectAccessLevelId=1,Name="Deny",Level = 10},
            new ObjectAccessLevel() { ObjectAccessLevelId=2,Name="Read",Level=1000}
        };

    }
}
