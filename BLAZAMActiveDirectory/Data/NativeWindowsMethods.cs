using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BLAZAM.ActiveDirectory.Data
{
    internal static class NativeWindowsMethods
    {


        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        internal static extern int NCryptStreamOpenToUnprotect(in NCRYPT_PROTECT_STREAM_INFO pStreamInfo, int dwFlags, IntPtr hWnd, out IntPtr phStream);

        [DllImport("ncrypt.dll")]
        internal static extern int NCryptStreamUpdate(IntPtr hStream, IntPtr pbData, int cbData, [MarshalAs(UnmanagedType.Bool)] bool fFinal);

        [DllImport("ncrypt.dll")]
        internal static extern int NCryptStreamClose(IntPtr hStream);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int PFNCryptStreamOutputCallback(IntPtr pvCallbackCtxt, IntPtr pbData, int cbData, [MarshalAs(UnmanagedType.Bool)] bool fFinal);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct NCRYPT_PROTECT_STREAM_INFO
        {
            public PFNCryptStreamOutputCallback pfnStreamOutput;
            public IntPtr pvCallbackCtxt;
        }

    }
}
