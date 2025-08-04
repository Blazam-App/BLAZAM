using System;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using static MudBlazor.Colors;
using System.Xml.Linq;
using static BLAZAM.ActiveDirectory.Data.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Security;
using System.Diagnostics;

namespace BLAZAM.ActiveDirectory.Data
{
    internal class Win32
    {
        [Flags]
        public enum ProtectFlags
        {
            NCRYPT_SILENT_FLAG = 0x00000040,
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int PFNCryptStreamOutputCallback(IntPtr pvCallbackCtxt, IntPtr pbData, int cbData, [MarshalAs(UnmanagedType.Bool)] bool fFinal);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct NCRYPT_PROTECT_STREAM_INFO
        {
            public PFNCryptStreamOutputCallback pfnStreamOutput;
            public IntPtr pvCallbackCtxt;
        }

        [Flags]
        public enum UnprotectSecretFlags
        {
            NCRYPT_UNPROTECT_NO_DECRYPT = 0x00000001,
            NCRYPT_SILENT_FLAG = 0x00000040,
        }

        [DllImport("ncrypt.dll")]
        public static extern uint NCryptStreamOpenToUnprotect(in NCRYPT_PROTECT_STREAM_INFO pStreamInfo, ProtectFlags dwFlags, IntPtr hWnd, out IntPtr phStream);

        [DllImport("ncrypt.dll")]
        public static extern uint NCryptStreamUpdate(IntPtr hStream, IntPtr pbData, int cbData, [MarshalAs(UnmanagedType.Bool)] bool fFinal);

        [DllImport("ncrypt.dll")]
        public static extern uint NCryptUnprotectSecret(out IntPtr phDescriptor, Int32 dwFlags, IntPtr pbProtectedBlob, uint cbProtectedBlob, IntPtr pMemPara, IntPtr hWnd, out IntPtr ppbData, out uint pcbData);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        public static extern uint NCryptGetProtectionDescriptorInfo(IntPtr hDescriptor, IntPtr pMemPara, int dwInfoType, out string ppvInfo);
    }
    public class LapsDecryptor
    {

        public LapsDecryptor()
        {
        }

        public string Decrypt(byte[] encryptedPass)
        {
            if(encryptedPass.Length<17)
            {
                throw new ArgumentException("Encrypted password must be at least 17 bytes long.");
            }
            var tcs = new TaskCompletionSource<string>();
            GCHandle gch = GCHandle.Alloc(tcs);



            var info = new NCRYPT_PROTECT_STREAM_INFO
            {
                pfnStreamOutput = new PFNCryptStreamOutputCallback(delegateCallback),
                // Pass the handle as the context pointer.
                pvCallbackCtxt = GCHandle.ToIntPtr(gch)
            };

            IntPtr handle;
            IntPtr handle2;
            IntPtr secData;
            uint secDataLen;
            NTAccount ntaccount;

            uint ret = Win32.NCryptStreamOpenToUnprotect(info, ProtectFlags.NCRYPT_SILENT_FLAG, IntPtr.Zero, out handle);
            if (ret != 0)
            {
                throw new Win32Exception((int)ret);
            }
            IntPtr alloc = Marshal.AllocHGlobal(encryptedPass.Length);
            try
            {
                Marshal.Copy(encryptedPass, 16, alloc, encryptedPass.Length - 16);
                ret = Win32.NCryptUnprotectSecret(out handle2, 0x41, alloc, (uint)encryptedPass.Length - 16, IntPtr.Zero, IntPtr.Zero, out secData, out secDataLen);
                if (ret == 0)
                {
                    string sid;

                    ret = NCryptGetProtectionDescriptorInfo(handle2, IntPtr.Zero, 1, out sid);
                    if (ret == 0)
                    {
                        SecurityIdentifier securityIdentifier = new SecurityIdentifier(sid.Substring(4, sid.Length - 4));

                        try
                        {
                            ntaccount = (securityIdentifier.Translate(typeof(NTAccount)) as NTAccount);

                            Console.WriteLine("[*] Authorized Decryptor: {0}", ntaccount.ToString());
                        }
                        catch
                        {
                            Console.WriteLine("[*] Authorized Decryptor SID: {0}", securityIdentifier.ToString());
                        }
                    }
                }
                ret = Win32.NCryptStreamUpdate(handle, alloc, encryptedPass.Length - 16, true);
                if (ret != 0)
                {
                    throw new Win32Exception((int)ret);
                }
                return tcs.Task.GetAwaiter().GetResult();

            }
            finally
            {
                Marshal.FreeHGlobal(alloc); // Ensure memory is freed
            }



        }

        int delegateCallback(IntPtr pvCallbackCtxt, IntPtr pbData, int cbData, [MarshalAs(UnmanagedType.Bool)] bool fFinal)
        {
            try
            {
                // 1. Get the GCHandle back from the context pointer.
                GCHandle gch = GCHandle.FromIntPtr(pvCallbackCtxt);

                // 2. Get the TaskCompletionSource object.
                var tcs = (TaskCompletionSource<string>)gch.Target;

                byte[] data = new byte[cbData];
                Marshal.Copy(pbData, data, 0, cbData);
                string str = Encoding.Unicode.GetString(data);

                // 3. Set the result on the TaskCompletionSource.
                // This unblocks the 'await' in DecryptAsync and returns the value.
                tcs.SetResult(str);

                return 0; // Success
            }
            catch (Exception ex)
            {
                // Handle potential exceptions during callback processing
                GCHandle gch = GCHandle.FromIntPtr(pvCallbackCtxt);
                var tcs = (TaskCompletionSource<string>)gch.Target;
                tcs.SetException(ex); // Propagate the error to the awaiting caller
                return -1; // Indicate failure
            }
        }

    }
}