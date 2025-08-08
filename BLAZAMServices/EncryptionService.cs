using BLAZAM.Common.Data.Services;
using BLAZAM.Logger;
using Microsoft.Extensions.Configuration;

namespace BLAZAM.Services
{
    /// <summary>
    /// Provides encryption and decryption services using a centrally managed encryption key.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private Encryption Encryption { get; set; }

        /// <summary>
        /// Gets the API token encryption key.
        /// </summary>
        public byte[] Key => Encryption.APITokenKey;

        /// <summary>
        /// Gets the connection state of the service, indicating if the encryption key is loaded.
        /// </summary>
        public ServiceConnectionState Status
        {
            get
            {
                return Encryption.APITokenKey != null ? ServiceConnectionState.Up : ServiceConnectionState.Down;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionService"/> class, configuring the underlying encryption mechanism.
        /// </summary>
        /// <param name="configuration">The application configuration, used to retrieve the encryption key.</param>
        /// <exception cref="ArgumentNullException">Thrown if configuration is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the EncryptionKey is missing from configuration.</exception>
        public EncryptionService(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);


            var encryptionKey = configuration.GetValue<string>("EncryptionKey");

            if (string.IsNullOrEmpty(encryptionKey))
            {
                Loggers.SystemLogger.Fatal("EncryptionKey is null or empty in application configuration. EncryptionService cannot be initialized.");
                throw new InvalidOperationException("EncryptionKey is missing from configuration.");
            }

            if (Encryption.Instance == null)
            {
                try
                {
                    Encryption = new Encryption(encryptionKey);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Exception during new Encryption(encryptionKey) in EncryptionService constructor.");
                    throw; 
                }
            }
            else
            {
                Encryption = Encryption.Instance;
            }
        }

        /// <summary>
        /// Decrypts the provided ciphertext into an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to decrypt the object to.</typeparam>
        /// <param name="cipherText">The ciphertext to decrypt.</param>
        /// <returns>The decrypted object, or default(T) if decryption fails or input is null.</returns>
        public T? DecryptObject<T>(string? cipherText) => Encryption.DecryptObject<T>(cipherText);

        /// <summary>
        /// Encrypts the provided object into a ciphertext string.
        /// </summary>
        /// <param name="obj">The object to encrypt.</param>
        /// <returns>The ciphertext string, or null if encryption fails or input is null.</returns>
        public string EncryptObject(object obj) => Encryption.EncryptObject(obj);
    }
}
