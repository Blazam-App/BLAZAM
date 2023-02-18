namespace BLAZAM.Common.Models.Database.Permissions
{
    public class FieldAccessLevel
    {
        public int FieldAccessLevelId { get; set; }
        public string Name { get; set; }
        public int Level { get; internal set; }
        public virtual ICollection<FieldAccessMapping> FieldAccessMappings { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is FieldAccessLevel)
            {
                var o = obj as FieldAccessLevel;
                if (o.Name.Equals(Name)) return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string? ToString()
        {
            return Name;
        }
    }
}
