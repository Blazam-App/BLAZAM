using BLAZAM.Common.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class EncryptionHelpers
    {
        /// <summary>
        /// Decrypts this cipher text into the type provided
        /// </summary>
        /// <remarks>
        /// If decryption fails, the provided text is returned
        /// </remarks>
        /// <typeparam name="T">The type expected to be decrypted</typeparam>
        /// <param name="input">The cipher text</param>
        /// <returns></returns>
        public static T? Decrypt<T>(this string input)
        {
            return Encryption.Instance.DecryptObject<T>(input);
           
        }
        public static string Decrypt(this string input)
        {
            var str = Encryption.Instance.DecryptObject<string>(input);
            return str == null ? "" : str;
        }


        /// <summary>
        /// Encrpyts this object
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string? Encrypt(this object input)
        {
            return Encryption.Instance.EncryptObject(input);
        }
    }
}
