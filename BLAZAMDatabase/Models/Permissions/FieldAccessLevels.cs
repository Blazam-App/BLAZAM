namespace BLAZAM.Database.Models.Permissions
{
    public class FieldAccessLevels
    {
        public static List<FieldAccessLevel> Levels = new List<FieldAccessLevel>() {
                new FieldAccessLevel() { Id = 1, Name = "Deny", Level = 10 },
                new FieldAccessLevel() { Id = 2, Name = "Read", Level = 100 },
                new FieldAccessLevel() { Id = 3, Name = "Edit", Level = 1000 }
        };

        public static FieldAccessLevel Deny = new FieldAccessLevel() { Id = 1, Name = "Deny", Level = 10 };
        public static FieldAccessLevel Read = new FieldAccessLevel() { Id = 2, Name = "Read", Level = 100 };
        public static FieldAccessLevel Edit = new FieldAccessLevel() { Id = 3, Name = "Edit", Level = 1000 };
    }
}
