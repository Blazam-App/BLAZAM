using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory.Data
{
    public class ActiveDirectoryUserState
    {
        public string Username { get; set; }
        public List<PermissionMapping> PermissionMappings { get; set; }
        public bool IsSuperAdmin { get; set; }


        public bool HasPermission(
            string dnTarget,
            Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector,
            Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector,
            bool nestedSearch)
        {
            if (IsSuperAdmin) return true;

            IOrderedEnumerable<PermissionMapping> baseSearch = !nestedSearch
                ? PermissionMappings.Where(pm => dnTarget.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length)
                : PermissionMappings.Where(pm => pm.OU.Contains(dnTarget)).OrderByDescending(pm => pm.OU.Length);

            try
            {
                var possibleReads = allowSelector(baseSearch).ToList();
                if (possibleReads.Count == 0)
                    return false;

                if (denySelector == null)
                    return true;

                var possibleDenys = denySelector(baseSearch).ToList();
                if (possibleDenys.Count == 0)
                    return true;

                int maxReadLength = possibleReads.Max(r => r.OU.Length);
                bool hasDenyWithLongerOU = possibleDenys.Any(d => d.OU.Length > maxReadLength);

                return !hasDenyWithLongerOU;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message);
                return false;
            }
        }

        public bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType)
        {
            return HasPermission(dnTarget, p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              am.AllowOrDeny && am.ObjectAction.Id == action.Id &&
              am.ObjectType == objectType
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              !am.AllowOrDeny && am.ObjectAction.Id == action.Id &&
              am.ObjectType == objectType
               ))), false
               );
        }
    }
}
