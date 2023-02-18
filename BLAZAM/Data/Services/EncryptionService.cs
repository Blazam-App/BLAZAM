using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace BLAZAM.Server.Data.Services
{
    public class EncryptionService
    {

        private readonly string _keyFilePath;
        private readonly int _keySize;
        private readonly int _blockSize;
        private byte[] _key;

        public EncryptionService(int keySize = 256, int blockSize = 128)
        {
            _keyFilePath = Program.WritablePath+@"security\database.key";
            _keySize = keySize;
            _blockSize = blockSize;
            LoadKey();
        }

        private void LoadKey()
        {
            if (File.Exists(_keyFilePath))
            {
                _key = File.ReadAllBytes(_keyFilePath);
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
                aes.KeySize = _keySize;
                aes.BlockSize = _blockSize;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateKey();
                _key = aes.Key;
                File.WriteAllBytes(_keyFilePath, aes.Key);
            }
        }


        public T DecryptObject<T>(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
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
        public string EncryptObject<T>(T obj)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
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
