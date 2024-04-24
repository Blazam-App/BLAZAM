using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using Microsoft.Extensions.Configuration;

namespace BLAZAM.Services
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

        public EncryptionService(IConfiguration configuration)
        {
            if (Encryption.Instance == null)
                Encryption = new Encryption(configuration.GetValue<string>("EncryptionKey"));
            else
            {
                Encryption = Encryption.Instance;
            }
        }



        public T? DecryptObject<T>(string? cipherText) => Encryption.DecryptObject<T>(cipherText);
        public string EncryptObject(object obj) => Encryption.EncryptObject(obj);


    }
}
