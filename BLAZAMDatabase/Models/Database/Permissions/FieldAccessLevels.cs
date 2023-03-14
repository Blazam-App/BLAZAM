namespace BLAZAM.Database.Models.Database.Permissions
{
    public class FieldAccessLevels
    {
        public static List<FieldAccessLevel> Levels = new List<FieldAccessLevel>() { 
                new FieldAccessLevel() { FieldAccessLevelId = 1, Name = "Deny", Level = 10 },
                new FieldAccessLevel() { FieldAccessLevelId = 2, Name = "Read", Level = 100 },
                new FieldAccessLevel() { FieldAccessLevelId = 3, Name = "Edit", Level = 1000 } 
        };

        public static FieldAccessLevel Deny = new FieldAccessLevel() { FieldAccessLevelId=1,Name="Deny", Level=10};
        public static FieldAccessLevel Read = new FieldAccessLevel() { FieldAccessLevelId=2,Name="Read", Level=100};
        public static FieldAccessLevel Edit = new FieldAccessLevel() { FieldAccessLevelId=3,Name="Edit", Level=1000};
    }
}
