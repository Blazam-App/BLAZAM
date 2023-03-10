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

        [Theory]
        [InlineData("s")]
        [InlineData("sh")]
        [InlineData("sho")]
        [InlineData("shor")]
        [InlineData("short")]
        [InlineData("longlonglonglong")]
        [InlineData("longlonglonglonglonglonglonglonglonglonglonglong")]
        [InlineData("longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong")]
        [InlineData("~!@#$%^&*()_+QWERTYUIP{}ASDFGHJKL:::ZXCVBNM<>?123456789/*-+0.qwertyuiop[]asdfghjkl;zxcvbnm,./`-=")]
        [InlineData(" ")]
        [InlineData("                                   ")]
        [InlineData("a                                   z")]
        public void CanEncrypt_String(string value)
        {
            var test = value;
            var cipher = encryption.EncryptObject(test);
            var result = encryption.DecryptObject<string>(cipher);
            Assert.Equal(test, result);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-123456789123456789)]
        [InlineData(123456789123456789)]
        public void CanEncrypt_Integer(int value)
        {
            int test = value;
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
