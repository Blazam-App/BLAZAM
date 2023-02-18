namespace BLAZAM.Common.Models.Database.Permissions
{
    public class ObjectAccessLevel
    {
        public int ObjectAccessLevelId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public virtual ICollection<ObjectAccessMapping> ObjectAccessMappings { get; set; }

    }
}
