using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Common.Models.Database
{
    public class ActiveDirectoryFieldObjectType : AppDbSetBase
    {
        public ActiveDirectoryObjectType ObjectType { get; set; }
        public int ActiveDirectoryFieldId { get;  set; }
    }
}