using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class ThemeHelpers
    {
        public static string ToHex(this System.Drawing.Color color)
        {
            string rtn = string.Empty;
            try
            {
                rtn = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
            }
            catch
            {
                //doing nothing
            }

            return rtn;
        }
    }
}
