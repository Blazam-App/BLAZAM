using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BLAZAM.ActiveDirectory.Data
{
    public class LapsDecryptor
    {
        #region Native P/Invoke Definitions

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptStreamOpenToUnprotect(
            [In] ref NCRYPT_PROTECT_STREAM_INFO pStreamInfo,
            int dwFlags,
            IntPtr hWnd,
            out IntPtr phStream);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptStreamUpdate(
            IntPtr hStream,
            IntPtr pbData,
            int cbData,
            [MarshalAs(UnmanagedType.Bool)] bool fFinal);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptStreamClose(IntPtr hStream);

        [StructLayout(LayoutKind.Sequential)]
        private struct NCRYPT_PROTECT_STREAM_INFO
        {
            public PF_NCRYPT_PROTECT_STREAM_OUTPUT_CALLBACK pfnStreamOutput;
            public IntPtr pvCallbackCtxt;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PF_NCRYPT_PROTECT_STREAM_OUTPUT_CALLBACK(
            IntPtr pvCallbackCtxt,
            IntPtr pbData,
            int cbData,
            [MarshalAs(UnmanagedType.Bool)] bool fFinal);

        // A security context is required for Windows LAPS decryption
        private const int NCRYPT_SILENT_FLAG = 0x00000040;

        #endregion

        private MemoryStream _decryptedStream;

        /// <summary>
        /// The callback function that receives decrypted data chunks from the NCrypt API.
        /// </summary>
        private int DecryptionCallback(IntPtr pvCallbackCtxt, IntPtr pbData, int cbData, bool fFinal)
        {
            if (cbData > 0)
            {
                byte[] buffer = new byte[cbData];
                Marshal.Copy(pbData, buffer, 0, cbData);
                _decryptedStream.Write(buffer, 0, cbData);
            }
            return 0; // SUCCESS
        }

        /// <summary>
        /// Decrypts a Windows LAPS password retrieved from the ms-LAPS-Password attribute.
        /// </summary>
        /// <param name="encryptedPassword">The raw byte array from the AD attribute.</param>
        /// <returns>A JSON string containing the password and other metadata.</returns>
        public string Decrypt(byte[] encryptedPassword)
        {
            if (encryptedPassword == null || encryptedPassword.Length == 0)
            {
                // Return null or throw, depending on desired behavior for empty passwords.
                return null;
            }

            // **FIX**: Reset state by initializing a new MemoryStream for each call.
            _decryptedStream = new MemoryStream();

            // Allocate unmanaged memory for the entire encrypted blob.
            IntPtr unmanagedData = Marshal.AllocHGlobal(encryptedPassword.Length);

            try
            {
                // **FIX**: Copy the ENTIRE encrypted blob, including the header. Do not skip any bytes.
                Marshal.Copy(encryptedPassword, 0, unmanagedData, encryptedPassword.Length);

                var streamInfo = new NCRYPT_PROTECT_STREAM_INFO
                {
                    pfnStreamOutput = DecryptionCallback,
                    pvCallbackCtxt = IntPtr.Zero
                };

                // **NOTE**: NCRYPT_SILENT_FLAG is required as there is no user context for a protection descriptor.
                int result = NCryptStreamOpenToUnprotect(ref streamInfo, NCRYPT_SILENT_FLAG, IntPtr.Zero, out IntPtr streamHandle);
                if (result != 0)
                {
                    throw new Win32Exception(result, "Failed to open LAPS decryption stream. Ensure the executing user has permissions.");
                }

                try
                {
                    // **FIX**: Pass the length of the entire blob.
                    result = NCryptStreamUpdate(streamHandle, unmanagedData, encryptedPassword.Length, true);
                    if (result != 0)
                    {
                        throw new Win32Exception(result, "Failed to update LAPS decryption stream.");
                    }
                }
                finally
                {
                    NCryptStreamClose(streamHandle);
                }

                // The decrypted data is a Unicode (UTF-16LE) JSON string.
                return Encoding.Unicode.GetString(_decryptedStream.ToArray());
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedData);
                _decryptedStream?.Dispose();
            }
        }
    }
}