using BLAZAM.Common.Data;

namespace BLAZAM.Database.Models
{
    public class ActiveDirectoryFieldObjectType : AppDbSetBase
    {
        public ActiveDirectoryObjectType ObjectType { get; set; }
        public int ActiveDirectoryFieldId { get; set; }
    }
}