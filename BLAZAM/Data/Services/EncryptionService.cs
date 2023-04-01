using BLAZAM.Common.Data;
using BLAZAM.Server.Background;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace BLAZAM.Server.Data.Services
{
    public class EncryptionService : IEncryptionService
    {
        private Encryption Encryption { get; set; }

        public ServiceConnectionState Status
        {
            get
            {
                return Encryption.Key != null ? ServiceConnectionState.Up : ServiceConnectionState.Down;
            }
        }

        public EncryptionService()
        {
            Encryption = new Encryption(Program.Configuration?.GetValue<string>("EncryptionKey"));
        }



        public T? DecryptObject<T>(string? cipherText) => Encryption.DecryptObject<T>(cipherText);
        public string EncryptObject(object obj) => Encryption.EncryptObject(obj);


    }
}
