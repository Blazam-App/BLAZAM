using BLAZAM.Common.Data;
using BLAZAM.Helpers;

namespace BLAZAM.Tests
{
    public class EncryptionTests
    {
        /// <summary>
        /// This really has no reason to ever change,
        /// but if it does, update the CanDecrypt
        /// method's test cipherString
        /// 
        /// </summary>
        /// <remarks>Value: thisisaseedkeystring</remarks>
        private const string TestSeedString = "thisisaseedkeystring";
        Encryption encryption;

        public EncryptionTests()
        {
            this.encryption = new Encryption(TestSeedString);
        }


        [Fact]
        public void CanEncrypt()
        {
            var test = "test";
            var result = encryption.EncryptObject(test);
            Assert.NotEqual<string>(test, result);
        }
        [Fact]
        public void CanDecrypt()
        {
            var test = "0qHlU8ZdxuW4Vp3BiNRJubp0mzxFXRf8+pyaUdSENv4=";
            var expected = "test";
            var result = encryption.DecryptObject<string>(test);
            Assert.Equal<string>(expected, result);
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
        [InlineData(-1233456789)]
        [InlineData(1233456789)]
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
        List<string> testSeedStrings = new List<string> {TestSeedString,
        "differentseedkeystring",
        "differentseedkeystrin",
        "diferentseedkeystring",
        "differetseedkeystring",
        "differentseedeystring",
        "differentseedkeytring",
        "seedk3ystr1ngw1th4numbers",
        "Seedk3ystr1ngw1th$pe$#*%(characters",
        "small",
        "t",
        "a",
        "f",
        "6",
        "@",
        "reallyreallyreallyreallyreallyreallyreallyreallyreallyreallyreallyreallylongseedstring"
        };

        [Fact]
        public void Accetable_KeyVariance()
        {
            List<byte[]> generatedKeys = new List<byte[]>();

            testSeedStrings.ForEach(seedString =>
            {
                encryption = new Encryption(seedString);
                generatedKeys.Add(encryption.Key);
            });

            List<int> lowestVariances = new List<int>();
            generatedKeys.ForEach(key =>
            {
                //Compare against all other keys and return lowest variance value
                int lowestVariance = int.MaxValue;
                generatedKeys.Where(k => !k.SequenceEqual(key)).ToList().ForEach(otherKey =>
                {
                    //Calculate xor of the two 256 bit keys
                    int variance = key.BitDifference(otherKey);
                    //Update lowestVariance if needed
                    if (variance < lowestVariance) lowestVariance = variance;
                });
                lowestVariances.Add(lowestVariance);
            });
            int lowestVarianceValue = lowestVariances.OrderBy(v => v).First();
            int lowestIndex = lowestVariances.IndexOf(lowestVarianceValue);


            Assert.True(lowestVarianceValue > 90);
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        public void Key_Null_ForInvalid_EncryptionKeyString(string? seedString)
        {

            encryption = new Encryption(seedString);
            Assert.Null(encryption.Key);
            
        }
    }
}
