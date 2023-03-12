using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace BLAZAM.Server.Data.Services
{
    public class EncryptionService:IEncryptionService
    {
        private Encryption Encryption { get; set; }


        public EncryptionService()
        {
            Encryption = new Encryption(Program.Configuration?.GetValue<string>("EncryptionKey"));
        }



        public T? DecryptObject<T>(string? cipherText) => Encryption.DecryptObject<T>(cipherText);
        public string EncryptObject(object obj) => Encryption.EncryptObject(obj);


    }
}
