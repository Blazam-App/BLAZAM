using BLAZAM.Server.Data.Services;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;

namespace BLAZAM.Common.Data
{
    internal class Encryption
    {
        private static Encryption instance;

        public string KeyFilePath { get; set; }
        public int KeySize { get; set; }
        public int KeyBlockSize { get; set; }
        public byte[] Key { get; set; }

        public Encryption(string? keyFilePath= null, int keySize=256, int keyBlockSize=128)
        {
            if (keyFilePath == null) keyFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+@"writable\security\database.key";
            KeyFilePath = keyFilePath;
            KeySize = keySize;
            KeyBlockSize = keyBlockSize;
            LoadKey();
            instance = this;

        }
        private void LoadKey()
        {
            if (File.Exists(KeyFilePath))
            {
                Key = File.ReadAllBytes(KeyFilePath);
            }
            else
            {
                GenerateAesKey();
            }
        }

        private async void GenerateAesKey()
        {
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = KeyBlockSize;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateKey();
                Key = aes.Key;
                Directory.CreateDirectory(Path.GetDirectoryName(KeyFilePath));
                File.WriteAllBytes(KeyFilePath, aes.Key);
            }
        }

        public static T DecryptObject<T>(string cipherText)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = instance.Key;
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
                if(cipherText is T tText)
                {
                    return tText;
                }
            }
            throw new ApplicationException("Unable to decrypt cipherText");
        }
        public static string EncryptObject(object obj)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = instance.Key;
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