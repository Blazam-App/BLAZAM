using BLAZAM.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests
{
    public class EncryptionTests
    {
        Encryption encryption;

        public EncryptionTests()
        {
            this.encryption = new Encryption("thisisaseedkeystring");
        }

        [Fact]
        public void CanEncrypt_String()
        {
            var test = "test";
            var cipher = encryption.EncryptObject(test);
            var result = encryption.DecryptObject<string>(cipher);
            Assert.Equal(test, result);

        }

        [Fact]
        public void CanEncrypt_Integer()
        {
            int test = 43653456;
            var cipher = encryption.EncryptObject(test);
            var result = encryption.DecryptObject<int>(cipher);
            Assert.Equal(test, result);

        }

        [Fact]
        public void CanEncrypt_ClassObject()
        {
            var test = new Uri("https://google.com");
            var cipher = encryption.EncryptObject(test);
            var result = encryption.DecryptObject<Uri>(cipher);
            Assert.Equal(test, result);

        }

        [Fact]
        public void CanEncrypt_EmptyString()
        {
            var test = "";
            var cipher = encryption.EncryptObject(test);
            var result = encryption.DecryptObject<string>(cipher);
            Assert.Equal(test, result);

        }
        [Fact]
        public void CanEncrypt_Null()
        {
            string? test = null;
            var cipher = encryption.EncryptObject(test);
            var result = encryption.DecryptObject<string>(cipher);
            Assert.Equal(test, result);

        }
    }
}
