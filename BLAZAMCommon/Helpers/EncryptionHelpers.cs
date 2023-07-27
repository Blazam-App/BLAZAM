using BLAZAM.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class EncryptionHelpers
    {

        public static string Decrypt(this string input)
        {
            var str= Encryption.Instance.DecryptObject<string>(input);
            return str == null ? "" : str;
        }



        public static string? Encrypt(this object input)
        {
            return Encryption.Instance.EncryptObject(input);
        }
    }
}
