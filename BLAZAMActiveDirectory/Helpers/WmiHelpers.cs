using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Helpers
{
    public static class WmiHelpers
    {
        public static T? GetPropertyValue<T>(this ManagementObject? mo, string propertyName)
        {
            var value = mo.GetPropertyValue(propertyName);
            if (value is T) { return (T)value; }
            return default;

        }
    }
}
