using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BLAZAM.Database.Models.Permissions
{
    public class PermissionMapping : RecoverableAppDbSetBase
    {

        public IEnumerable<PermissionDelegate> PermissionDelegates { get; set; }
        public IEnumerable<AccessLevel> AccessLevels { get; set; }
        public string OU { get; set; }
    }
}
