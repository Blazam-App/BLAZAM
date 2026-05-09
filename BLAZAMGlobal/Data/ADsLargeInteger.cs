using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Global.Data
{
   
    /// <summary>
    /// Represents the ADSI LargeInteger structure, used for date/time and other large integer values in Active Directory.
    /// This interface is used for COM interop with ADSI.
    /// </summary>
    [
        ComImport,
        Guid("9068270b-0939-11d1-8be1-00c04fd8d503"),
        InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
    ]
    public interface IADsLargeInteger
    {
        /// <summary>Gets or sets the high part of the large integer.</summary>
        [DispId(2)] int HighPart { get; set; }
        /// <summary>Gets or sets the low part of the large integer.</summary>
        [DispId(3)] int LowPart { get; set; }
    }

    /// <summary>
    /// A managed representation of the ADSI IADsLargeInteger, primarily for testing or scenarios where COM interop is not directly available.
    /// </summary>
    public class ADsLargeInteger : IADsLargeInteger
    {

        /// <summary>
        /// Represents the "null" or earliest possible date in ADSI time (January 1, 1601, 12:00:00 AM UTC).
        /// </summary>
        public static DateTime ADS_NULL_TIME
        {
            get
            {
                // Use TryParseExact for robustness if needed, but this format is standard.
                var ads_null_time = DateTime.ParseExact("01/01/1601 12:00:00 AM", "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                return DateTime.SpecifyKind(ads_null_time, DateTimeKind.Utc);
            }
        }


        /// <summary>Gets or sets the high part of the large integer.</summary>
        public int HighPart { get; set; }
        /// <summary>Gets or sets the low part of the large integer.</summary>
        public int LowPart { get; set; }
    }
}
