using BLAZAM.Common.Data;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides extension methods to simplify encryption and decryption operations using the application's central <see cref="BLAZAM.Common.Data.Encryption"/> service.
    /// </summary>
    public static class EncryptionHelpers
    {
        /// <summary>
        /// Decrypts a ciphertext string into an object of the specified type.
        /// </summary>
        /// <remarks>
        /// Relies on <see cref="BLAZAM.Common.Data.Encryption.Instance"/>. If decryption fails (e.g., due to invalid format, incorrect key, or if the input is null), this method returns default(T).
        /// </remarks>
        /// <typeparam name="T">The type to decrypt the object to.</typeparam>
        /// <param name="input">The ciphertext string to decrypt. If null, default(T) is returned.</param>
        /// <returns>The decrypted object, or default(T) if decryption fails or input is null.</returns>
        public static T? Decrypt<T>(this string input)
        {
            if (input == null)
            {
                return default(T);
            }
            return Encryption.Instance.DecryptObject<T>(input);
        }

        /// <summary>
        /// Decrypts a ciphertext string into a plain text string.
        /// </summary>
        /// <remarks>
        /// Relies on <see cref="BLAZAM.Common.Data.Encryption.Instance"/>. If decryption fails or the input is null, this method returns an empty string.
        /// </remarks>
        /// <param name="input">The ciphertext string to decrypt. If null, an empty string is returned.</param>
        /// <returns>The decrypted string, or an empty string if decryption fails or input is null.</returns>
        public static string Decrypt(this string input)
        {
            if (input == null)
            {
                return string.Empty;
            }
            var str = Encryption.Instance.DecryptObject<string>(input);
            return str == null ? "" : str;
        }

        /// <summary>
        /// Encrypts an object into a ciphertext string.
        /// </summary>
        /// <remarks>
        /// Relies on <see cref="BLAZAM.Common.Data.Encryption.Instance"/>.
        /// </remarks>
        /// <param name="input">The object to encrypt. If null, null is returned.</param>
        /// <returns>The ciphertext string, or null if the input object is null or encryption fails.</returns>
        public static string? Encrypt(this object input)
        {
            if (input == null)
            {
                return null;
            }
            return Encryption.Instance.EncryptObject(input);
        }
    }
}
