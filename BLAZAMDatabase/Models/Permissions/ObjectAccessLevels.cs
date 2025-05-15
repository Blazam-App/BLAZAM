using BLAZAM.Helpers;

namespace BLAZAM.Database.Models.Permissions
{
    public class ObjectAccessLevels
    {
        public static List<ObjectAccessLevel> Levels => typeof(ObjectAccessLevels).GetStaticProperties<ObjectAccessLevel>();

        public static readonly ObjectAccessLevel Deny = new() {
            Id = 1,
            Name = "Deny",
            Level = 10 
        };
        public static readonly ObjectAccessLevel Read = new() {
            Id = 2, 
            Name = "Read",
            Level = 1000 
        };
       

    }
}
