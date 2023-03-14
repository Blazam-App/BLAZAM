using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BLAZAM.Database.Models.Database.Permissions
{
    public class PermissionMap
    {
        public int PermissionMapId { get; set; }
        
        public List<PermissionDelegate> PermissionDelegates { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }
        public string OU { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
