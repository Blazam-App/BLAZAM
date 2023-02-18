using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
namespace BLAZAM.Common.Data.ActiveDirectory
{


    public sealed class AdsiUtils
    {
        private AdsiUtils() { }

        [
            ComImport,
            Guid("9068270b-0939-11d1-8be1-00c04fd8d503"),
            InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
        ]
        private interface IADsLargeInteger
        {
            [DispId(2)] int HighPart { get; set; }
            [DispId(3)] int LowPart { get; set; }
        }

        public static DateTime AdsDateValue(object value)
        {
            try
            {
                IADsLargeInteger v = value as IADsLargeInteger;
                if (null == v) return DateTime.MinValue;

                long dV = ((long)v.HighPart << 32) + (long)v.LowPart;
                return DateTime.FromFileTime(dV);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
