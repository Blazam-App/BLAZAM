using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class PermissionMapping : AppDbSetBase
    {
        
        public IEnumerable<PermissionDelegate> PermissionDelegates { get; set; }
        public IEnumerable<AccessLevel> AccessLevels { get; set; }
        public string OU { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
