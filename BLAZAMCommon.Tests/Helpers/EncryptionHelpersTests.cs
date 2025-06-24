using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAMCommon.Tests.Helpers
{
    public class MyTestObject
    {
        public string? Message { get; set; }
        public int Value { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is MyTestObject other)
            {
                return Message == other.Message && Value == other.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Message, Value);
        }
    }

    public class EncryptionHelpersTests
    {
        private readonly BLAZAM.Common.Data.Encryption? _originalEncryptionInstance;

        public EncryptionHelpersTests()
        {
            // Store the original instance if it exists, to restore it later.
            // This helps prevent test interference.
            _originalEncryptionInstance = new Encryption("seedstring"); // Assuming Encryption.Instance is static and gettable

           
        }

       


        // -------------------------------------
        // Tests for Decrypt<T>(this string input)
        // -------------------------------------

        [Fact]
        public void Decrypt_Generic_NullInput_ReturnsDefaultT()
        {
            // Arrange
            string? input = null;

            // Act
            var resultClass = input.Decrypt<MyTestObject>();
            var resultStruct = input.Decrypt<int>(); // Example with a value type

            // Assert
            Assert.Null(resultClass); // default(MyTestObject) is null
            Assert.Equal(default(int), resultStruct); // default(int) is 0
        }

        [Fact]
        public void Decrypt_Generic_InvalidCiphertext_ReturnsDefaultT()
        {
            // Arrange
            // This string should be something your Encryption.Instance cannot decrypt.
            // It might be an empty string if that's handled, or garbage data.
            string invalidCiphertext = "---THIS IS NOT VALID ENCRYPTED DATA---";

            // Act
            var resultClass = invalidCiphertext.Decrypt<MyTestObject>();
            var resultStruct = invalidCiphertext.Decrypt<int>();

            // Assert
            // This assumes your Encryption.Instance.DecryptObject<T>() returns default(T)
            // for invalid input rather than throwing an exception.
            // If it throws, these asserts should be Assert.Throws<YourExceptionType>(...).
            Assert.Null(resultClass);
            Assert.Equal(default(int), resultStruct);
        }

        // -------------------------------------
        // Tests for Decrypt(this string input) - string overload
        // -------------------------------------

        [Fact]
        public void Decrypt_String_NullInput_ReturnsEmptyString()
        {
            // Arrange
            string? input = null;

            // Act
            var result = input.Decrypt();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Decrypt_String_InvalidCiphertext_ReturnsInputString()
        {
            // Arrange
            string invalidCiphertext = "---INVALID BASE64 OR ENCRYPTION STRING---";

            // Act
            var result = invalidCiphertext.Decrypt();

            // Assert
            // This assumes your Encryption.Instance.DecryptObject<string>() returns null
            // for invalid input, which the helper then converts to string.Empty.
            // If DecryptObject<string>() throws, this should be Assert.Throws<...>(...).
            Assert.Equal(invalidCiphertext, result);
        }

       

        // -------------------------------------
        // Tests for Encrypt(this object input)
        // -------------------------------------

        [Fact]
        public void Encrypt_NullInput_ReturnsNull()
        {
            // Arrange
            object? input = null;

            // Act
            var result = input.Encrypt();

            // Assert
            Assert.Null(result);
        }

    }
}
