using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BLAZAM.Common.Models.Database.Permissions
{
    public class PrivilegeMap
    {
        public int PrivilegeMapId { get; set; }
        
        public List<PrivilegeLevel> PrivilegeLevels { get; set; }
        public List<AccessLevel> AccessLevels { get; set; }
        public string OU { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
