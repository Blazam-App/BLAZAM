namespace BLAZAM.Database.Models.Permissions
{
    public class ObjectAccessLevels
    {
        public static ObjectAccessLevel Deny = new() { Id = 1, Name = "Deny", Level = 10 };
        public static ObjectAccessLevel Read = new() { Id = 2, Name = "Read", Level = 1000 };
        public static List<ObjectAccessLevel> Levels = new() {
            new ObjectAccessLevel() { Id=1,Name="Deny",Level = 10},
            new ObjectAccessLevel() { Id=2,Name="Read",Level=1000}
        };

    }
}
