using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class NumberHelpers
    {

        /// <summary>
        /// Rounds a double-precision floating-point number to a specified number of fractional digits.
        /// </summary>
        /// <param name="number">The number to round.</param>
        /// <param name="decimalPlaces">The number of decimal places in the return value. Defaults to 0.</param>
        /// <returns>The number rounded to the specified number of decimal places.</returns>
        public static double Round(this double number, int decimalPlaces = 0)
        {
            return Math.Round(number, decimalPlaces);
        }

    }
}
