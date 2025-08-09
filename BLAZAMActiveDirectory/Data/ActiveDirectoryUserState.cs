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


        public bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch)
        {
            if (IsSuperAdmin) return true;

            IOrderedEnumerable<PermissionMapping>? baseSearch = null;
            if (!nestedSearch)
            {
                baseSearch = PermissionMappings
       .Where(pm => dnTarget.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length);

            }
            else
            {
                baseSearch = PermissionMappings
       .Where(pm => pm.OU.Contains(dnTarget)).OrderByDescending(pm => pm.OU.Length);

            }


            try
            {
                var possibleReads = allowSelector.Invoke(baseSearch).ToList();
                if (denySelector != null)
                {
                    var possibleDenys = denySelector.Invoke(baseSearch).ToList();

                    if (possibleReads != null && possibleReads.Count > 0)
                    {
                        if (possibleDenys != null && possibleDenys.Count > 0)
                        {
                            foreach (var d in possibleDenys)
                            {
                                if (d.OU.Length > possibleReads.OrderByDescending(r => r.OU.Length).First().OU.Length)
                                    return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return possibleReads?.Count > 0;
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message);
            }
            return false;
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
