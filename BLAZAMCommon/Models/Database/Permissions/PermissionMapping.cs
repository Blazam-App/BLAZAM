using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class PermissionMapping : AppDbSetBase
    {
        
        public List<PermissionDelegate> PermissionDelegates { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }
        public string OU { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
