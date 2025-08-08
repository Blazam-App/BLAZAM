using BLAZAM.Helpers;

namespace BLAZAM.Database.Models.Permissions
{
    public class FieldAccessLevels
    {
        public static List<FieldAccessLevel> Levels => typeof(FieldAccessLevels).GetStaticProperties<FieldAccessLevel>();


        public static readonly FieldAccessLevel Deny = new()
        {
            Id = 1,
            Name = "Deny",
            Level = 10
        };
        public static readonly FieldAccessLevel Read = new()
        {
            Id = 2,
            Name = "Read",
            Level = 100
        };
        public static readonly FieldAccessLevel Edit = new()
        {
            Id = 3,
            Name = "Edit",
            Level = 1000
        };
    }
}
