using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Data
{
    public class LapsDecryptor
    {
        private byte[] _decryptedData;
        private int _offset = 0;

        private int DecryptionCallback(IntPtr pvCallbackCtxt, IntPtr pbData, int cbData, bool fFinal)
        {
            if (cbData > 0)
            {
                if (_decryptedData == null)
                {
                    _decryptedData = new byte[cbData];
                }
                else
                {
                    Array.Resize(ref _decryptedData, _decryptedData.Length + cbData);
                }
                Marshal.Copy(pbData, _decryptedData, _offset, cbData);
                _offset += cbData;
            }
            return 0; // SUCCESS
        }

        public string Decrypt(byte[] encryptedPassword)
        {
            if (encryptedPassword == null || encryptedPassword.Length <= 16)
            {
                throw new ArgumentException("Invalid encrypted password data.", nameof(encryptedPassword));
            }

            // Skip the 16-byte header
            int dataLength = encryptedPassword.Length - 16;
            IntPtr unmanagedData = Marshal.AllocHGlobal(dataLength);

            try
            {
                Marshal.Copy(encryptedPassword, 16, unmanagedData, dataLength);

                var streamInfo = new NativeWindowsMethods.NCRYPT_PROTECT_STREAM_INFO
                {
                    pfnStreamOutput = DecryptionCallback,
                    pvCallbackCtxt = IntPtr.Zero
                };

                if (NativeWindowsMethods.NCryptStreamOpenToUnprotect(streamInfo, 0, IntPtr.Zero, out IntPtr streamHandle) != 0)
                {
                    throw new InvalidOperationException("Failed to open decryption stream.");
                }

                try
                {
                    if (NativeWindowsMethods.NCryptStreamUpdate(streamHandle, unmanagedData, dataLength, true) != 0)
                    {
                        throw new InvalidOperationException("Failed to update decryption stream.");
                    }
                }
                finally
                {
                    NativeWindowsMethods.NCryptStreamClose(streamHandle);
                }

                if (_decryptedData != null)
                {
                    // The decrypted data is a JSON string
                    return Encoding.Unicode.GetString(_decryptedData);
                }

                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedData);
            }
        }
    }
}
