﻿

namespace BLAZAM.Database.Models.Permissions
{
    public class ObjectAccessLevel : AppDbSetBase
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public virtual ICollection<ObjectAccessMapping> ObjectAccessMappings { get; set; }

    }
}
