using BLAZAM.Server.Data.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BLAZAM.Server.Data
{
    internal class Encryption
    {
        /// <summary>
        /// String used to generate the key bytes
        /// </summary>
        /// <remarks>
        /// This is not the actual key used but will be transformed
        /// in a consistent manner, into a reusable key
        /// </remarks>
        public string KeySeedString { get; set; }

        /// <summary>
        /// The size of the key to generate
        /// </summary>
        public int KeySize { get; set; }

        public int KeyBlockSize { get; set; }

        public byte[] Key { get; set; }

        public static Encryption Instance;

        public Encryption(string? keySeedString, int keySize = 256, int keyBlockSize = 128)
        {
            if (keySeedString == null) throw new ApplicationException("An ecryption seedsting must be provided");
            KeySeedString = keySeedString;
            KeySize = keySize;
            KeyBlockSize = keyBlockSize;
            GenerateKeyFromSeedString();
            Instance = this;
        }

        /// <summary>
        /// Generates a key of the configured key size, seeding the
        /// key from the appsettings configuration value "EncryptionKey"
        /// </summary>
        /// <remarks>
        /// Sets the local <see cref="Key"/> value to the newly generated key
        /// </remarks>
        /// <returns>The key based on the <see cref="KeySeedString"/></returns>
        private byte[] GenerateKeyFromSeedString()
        {
            // Use a key derivation function to generate a repeatable key
            var salt = Encoding.UTF8.GetBytes("BLAZAM_SALT");
            var keyGenerator = new Rfc2898DeriveBytes(KeySeedString, salt, 1000);
            Key = keyGenerator.GetBytes(KeySize / 8);
            return Key;
        }


        public T DecryptObject<T>(string cipherText)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
                            }
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                if (cipherText is T tText)
                {
                    return tText;
                }
            }
            throw new ApplicationException("Unable to decrypt cipherText");
        }
        public string EncryptObject(object obj)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(JsonConvert.SerializeObject(obj));
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

    }


}